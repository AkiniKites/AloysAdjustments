using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Configuration;
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
        }

        public static IKernel Kernel { get; }
        public static T Get<T>() => Kernel.Get<T>();

        public static Config Config => Kernel.Get<Config>();
        public static UserSettings Settings => Kernel.Get<UserSettings>();
        public static Decima Decima => Kernel.Get<Decima>();
        public static Archiver Archiver => Kernel.Get<Archiver>();
        public static Localization Localization => Kernel.Get<Localization>();
        
        public static Action<string> SetStatus { get; set; }
        public static Action<string> SetError { get; set; }

        public static void Bind<T>(T value)
        {
            Kernel.Rebind<T>()
                .ToMethod(ctx => value)
                .InSingletonScope();
        }
    }
}
