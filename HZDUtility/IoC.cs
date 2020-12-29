using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Design;
using Ninject;
using Ninject.Planning.Bindings.Resolvers;

namespace HZDUtility
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
        public static Decima Decima => Kernel.Get<Decima>();
        public static Localization Localization => Kernel.Get<Localization>();

        public static void Bind<T>(T value)
        {
            Kernel.Rebind<T>()
                .ToMethod(ctx => value)
                .InSingletonScope();
        }
    }
}
