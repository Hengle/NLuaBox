using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Chormatism;
using NLua;
using System.Drawing;
using NLuaBox.Binders;
using System.Threading.Tasks;



namespace NLuaBox
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		UIViewController viewController;
		Lua context = new Lua ();

		object controler = typeof(JLTextViewController);

		public UIWindow Window { get { return window; } set { window = value; } }
		public UIViewController ViewController { get { return viewController; } set { viewController = value; } }
		public Lua Context { get { return context; } }

		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//

		public class Foo
		{
			public int x = 10;
		}

		Task<Foo> GetFoo ()
		{
			return Task.Factory.StartNew (() => {
				Console.WriteLine ("Test");
				var x =  new Foo ();
				Console.WriteLine ("Foo");
				return x;
			});
		}

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			NLuaBoxBinder.RegisterNLuaBox(context);
			InitNLua ();

			var x = GetFoo ().Result;
			try {
				context.DoFile ("scripts/sandbox/main.lua");

				LuaFunction initFunction = context ["Init"] as LuaFunction;

				var res = initFunction.Call (this).First ();	

			} catch (Exception e) {
				Console.WriteLine (e.ToString());
				return false;
			}
			return true;
		}

		void InitNLua()
		{
			context.LoadCLRPackage ();

			var printOutputFunc = typeof(AppDelegate).GetMethod ("Print");
			context.RegisterFunction ("print", this, printOutputFunc);

			context.DoString ("package.path = package.path .. \";./scripts/sandbox/?.lua\"");
		}

		public void Print(string output, params object[] extra )
		{
			Console.WriteLine (output);
		}

		protected override void Dispose (bool disposing)
		{
			if (context != null && disposing) {
				context.Dispose ();
				context = null;
			}

			base.Dispose (disposing);
		}
	}
}

