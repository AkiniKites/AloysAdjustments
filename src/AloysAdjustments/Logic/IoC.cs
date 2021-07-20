using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
using AloysAdjustments.Utility;
using Ninject;
using Ninject.Planning.Bindings.Resolvers;

namespace AloysAdjustments.Logic
{
    public class IoC
    {
        static IoC()
        {
            Kernel = new StandardKernel(
                new NinjectSettings
                {
                    LoadExtensions = false
                });
            Kernel.Components.Remove<IMissingBindingResolver, SelfBindingResolver>();

            var entry = Assembly.GetEntryAssembly();
            if (entry != null)
                Bind(entry.GetName().Version);
        }

        public static IKernel Kernel { get; }
        public static T Get<T>() => Kernel.Get<T>();

        public static Config Config => Kernel.Get<Config>();
        public static UserSettings Settings => Kernel.Get<UserSettings>();
        public static DebugConfig Debug => Kernel.Get<DebugConfig>();
        public static Archiver Archiver => Kernel.Get<Archiver>();
        public static Localization Localization => Kernel.Get<Localization>();
        public static Notifications Notif
        {
            get
            {
                var notif = Kernel.TryGet<Notifications>();
                if (notif == null)
                    notif = new Notifications((x, y) => { }, x => { }, (x, y, z, w) => { });
                return notif;
            }
        }
        public static Uuid Uuid => Kernel.Get<Uuid>();
        public static Version CurrentVersion => Kernel.Get<Version>(); 

        public static void Bind<T>(T value)
        {
            Kernel.Rebind<T>()
                .ToMethod(ctx => value)
                .InSingletonScope();
        }
    }
}
