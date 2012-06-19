using System;
using MonoTouch.Dialog;
using System.Threading;
using MonoTouch.UIKit;
using RedPlum;
using System.Linq;

namespace BitbucketBrowser.UI
{
    public abstract class Controller<T> : DialogViewController
    {
        public T Model { get; set; }

        private bool _loaded = false;

        public bool Loaded { get { return _loaded; } }

        public Controller(bool push = false, bool refresh = false)
            : base(new RootElement(""), push)
        {
            //View.BackgroundColor = UIColor.FromRGB(0.85f, 0.85f, 0.85f);
            if (refresh)
                RefreshRequested += (sender, e) => Refresh(true);

            NavigationItem.BackBarButtonItem = new UIBarButtonItem("Back", UIBarButtonItemStyle.Plain, null);
        }

        protected abstract void OnRefresh();

        protected abstract T OnUpdate();

        public override void ViewDidLoad()
        {
            Root.Caption = this.Title;
            base.ViewDidLoad();
        }

        public void Refresh(bool force = false)
        {
            if (Model != null && !force)
            {
                OnRefresh();
                InvokeOnMainThread(delegate { 
                    ReloadComplete(); 
                });
                _loaded = true;
                return;
            }

            MBProgressHUD hud = null;
            if (!force) {
                hud = new MBProgressHUD(this.View.Superview); 
                hud.Mode = MBProgressHUDMode.Indeterminate;
                hud.TitleText = "Loading...";
                this.View.Superview.AddSubview(hud);
                hud.Show(true);
            }
            ThreadPool.QueueUserWorkItem(delegate {
                Model = OnUpdate();
                Refresh();
                if (hud != null)
                {
                    InvokeOnMainThread(delegate {
                        hud.Hide(true);
                        hud.RemoveFromSuperview();
                    });
                }
            });
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (!_loaded)
            {
                Refresh();
                _loaded = true;
            }
        }
    }
}

