// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the HomeView type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Android.Content.PM;
using Android.Content.Res;
using Android.Support.V4.Widget;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Droid.Fragging;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using V.FlyoutTest.Core.Entities;
using V.FlyoutTest.Core.ViewModels;
using V.FlyoutTest.Droid.Helpers;

namespace V.FlyoutTest.Droid.Views
{
    using Android.App;
    using Android.OS;

    /// <summary>
    /// Defines the HomeView type.
    /// </summary>
    [Activity(Label = "Home", LaunchMode = LaunchMode.SingleTop)] //, Theme = "@style/MyTheme", Icon = "@drawable/ic_launcher"
    public class HomeView : MvxFragmentActivity, IFragmentHost
    {
        private DrawerLayout _drawer;
        private MyActionBarDrawerToggle _drawerToggle;
        private string _drawerTitle;
        private string _title;
        private MvxListView _drawerList;

        private HomeViewModel viewModel;
        public new HomeViewModel ViewModel
        {
            get { return this.viewModel ?? (this.viewModel = base.ViewModel as HomeViewModel); }
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.HomeView);

            this._title = this._drawerTitle = this.Title;
            this._drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            this._drawerList = this.FindViewById<MvxListView>(Resource.Id.left_drawer);

            this._drawer.SetDrawerShadow(Resource.Drawable.drawer_shadow_dark, (int)GravityFlags.Start);

            this.ActionBar.SetDisplayHomeAsUpEnabled(true);
            this.ActionBar.SetHomeButtonEnabled(true);

            //DrawerToggle is the animation that happens with the indicator next to the
            //ActionBar icon. You can choose not to use this.
            this._drawerToggle = new MyActionBarDrawerToggle(this, this._drawer,
                                                      Resource.Drawable.ic_drawer_light,
                                                      Resource.String.drawer_open,
                                                      Resource.String.drawer_close);

            //You can alternatively use _drawer.DrawerClosed here
            this._drawerToggle.DrawerClosed += delegate
            {
                this.ActionBar.Title = this._title;
                this.InvalidateOptionsMenu();
            };


            //You can alternatively use _drawer.DrawerOpened here
            this._drawerToggle.DrawerOpened += delegate
            {
                this.ActionBar.Title = this._drawerTitle;
                this.InvalidateOptionsMenu();
            };

            this._drawer.SetDrawerListener(this._drawerToggle);


            this.RegisterForDetailsRequests();

            if (null == savedInstanceState)
            {
                this.ViewModel.SelectMenuItemCommand.Execute(this.ViewModel.MenuItems[0]);
            }
            else
            {
                //restore viewModels if we have them
                RestoreViewModels();
            }
        }


        /// <summary>
        /// Use the custom presenter to determine if we can navigate forward.
        /// </summary>
        private void RegisterForDetailsRequests()
        {
            var customPresenter = Mvx.Resolve<ICustomPresenter>();
            customPresenter.Register(typeof(EnterTimeViewModel), this);
            customPresenter.Register(typeof(CreateNewJobViewModel), this);            
        }

        /// <summary>
        /// Read all about this, but this is a nice way if there were multiple
        /// fragments on the screen for us to decide what and where to show stuff
        /// See: http://enginecore.blogspot.ro/2013/06/more-dynamic-android-fragments-with.html
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool Show(MvxViewModelRequest request)
        {
            try
            {
                MvxFragment frag = null;
                var title = string.Empty;
                var section = this.ViewModel.GetSectionForViewModelType(request.ViewModelType);

                switch (section)
                {
                    case HomeViewModel.Section.EnterTime:
                        {
                            if (this.SupportFragmentManager.FindFragmentById(Resource.Id.content_frame) as EnterTimeView != null)
                            {
                                return true;
                            }

                            frag = new EnterTimeView();
                            title = "Enter Time";
                        }
                        break;
                    case HomeViewModel.Section.CreateNewJob:
                        {
                            if (this.SupportFragmentManager.FindFragmentById(Resource.Id.content_frame) as CreateNewJobView != null)
                            {
                                return true;
                            }

                            frag = new CreateNewJobView();
                            title = "Create New Job";
                        }
                        break;                   
                }

                var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
                var viewModelLocal = loaderService.LoadViewModel(request, null /* saved state */);

                frag.ViewModel = viewModelLocal;

                // TODO - replace this with extension method when available

                //Normally we would do this, but we already have it
                this.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, frag, title).Commit();
                this._drawerList.SetItemChecked(this.ViewModel.MenuItems.FindIndex(m => m.Id == (int)section), true);
                this.ActionBar.Title = this._title = title;

                this._drawer.CloseDrawer(this._drawerList);

                return true;
            }
            finally
            {
                this._drawer.CloseDrawer(this._drawerList);
            }
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this._drawerToggle.SyncState();
        }


        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            this._drawerToggle.OnConfigurationChanged(newConfig);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            //MenuInflater.Inflate(Resource.Menu.main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            var drawerOpen = this._drawer.IsDrawerOpen(this._drawerList);
            //when open down't show anything
            for (int i = 0; i < menu.Size(); i++)
                menu.GetItem(i).SetVisible(!drawerOpen);


            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (this._drawerToggle.OnOptionsItemSelected(item))
                return true;

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            SaveViewModelStates();
            base.OnSaveInstanceState(outState);
        }

        private void SaveViewModelStates()
        {
            //save all of the ViewModels for fragments
            var view = this.SupportFragmentManager.FindFragmentByTag("Enter Time") as EnterTimeView;
            if (null != view)
            {
                ViewModel.EnterTimeViewModelFragment = view.ViewModel as EnterTimeViewModel;
            }
            var view2 = this.SupportFragmentManager.FindFragmentByTag("Create New Job") as CreateNewJobView;
            if (null != view2)
            {
                ViewModel.CreateNewJobViewModelFragment = view2.ViewModel as CreateNewJobViewModel;
            }            
        }

        /// <summary>
        /// Restore view models to fragments if we have them and the fragments were created
        /// </summary>
        private void RestoreViewModels()
        {
            var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
            
            var view = this.SupportFragmentManager.FindFragmentByTag("Enter Time") as EnterTimeView;
            if (null != view && null == view.ViewModel)
            {
                view.ViewModel = ViewModel.EnterTimeViewModelFragment ?? loaderService.LoadViewModel(new MvxViewModelRequest(typeof(EnterTimeViewModel), null, null, null), null) as EnterTimeViewModel;
            }
            var view2 = this.SupportFragmentManager.FindFragmentByTag("Create New Job") as CreateNewJobView;
            if (null != view2 && null == view2.ViewModel)
            {
                view2.ViewModel = ViewModel.CreateNewJobViewModelFragment ?? loaderService.LoadViewModel(new MvxViewModelRequest(typeof(CreateNewJobViewModel), null, null, null), null) as CreateNewJobViewModel;
            }            
        }
    }
}