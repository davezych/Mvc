// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Security;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.NestedProviders;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    public class MvcServices
    {
        public static IEnumerable<IServiceDescriptor> GetDefaultServices()
        {
            return GetDefaultServices(new Configuration());
        }

        public static IEnumerable<IServiceDescriptor> GetDefaultServices(IConfiguration configuration)
        {
            var describe = new ServiceDescriber(configuration);

            yield return describe.Transient<IOptionsSetup<MvcOptions>, MvcOptionsSetup>();

            yield return describe.Transient<IControllerFactory, DefaultControllerFactory>();
            yield return describe.Singleton<IControllerActivator, DefaultControllerActivator>();
            yield return describe.Scoped<IActionSelector, DefaultActionSelector>();
            yield return describe.Transient<IActionInvokerFactory, ActionInvokerFactory>();
            yield return describe.Transient<IControllerAssemblyProvider, DefaultControllerAssemblyProvider>();
            yield return describe.Transient<IActionDiscoveryConventions, DefaultActionDiscoveryConventions>();

            yield return describe.Instance<IMvcRazorHost>(new MvcRazorHost(typeof(RazorView).FullName));

            yield return describe.Transient<ICompilationService, RoslynCompilationService>();

            yield return describe.Singleton<IViewEngineProvider, DefaultViewEngineProvider>();
            yield return describe.Scoped<ICompositeViewEngine, CompositeViewEngine>();
            yield return describe.Singleton<IRazorCompilationService, RazorCompilationService>();

            yield return describe.Singleton<IRazorViewActivator, RazorViewActivator>();
            // Virtual path view factory needs to stay scoped so views can get get scoped services.
            yield return describe.Scoped<IVirtualPathViewFactory, VirtualPathViewFactory>();
            yield return describe.Singleton<IFileInfoCache, ExpiringFileInfoCache>();

            yield return describe.Transient<INestedProvider<ActionDescriptorProviderContext>,
                                            ReflectedActionDescriptorProvider>();
            yield return describe.Transient<INestedProvider<ActionInvokerProviderContext>,
                                            ReflectedActionInvokerProvider>();
            yield return describe.Singleton<IActionDescriptorsCollectionProvider,
                DefaultActionDescriptorsCollectionProvider>();

            yield return describe.Transient<IModelMetadataProvider, DataAnnotationsModelMetadataProvider>();
            yield return describe.Scoped<IActionBindingContextProvider, DefaultActionBindingContextProvider>();

            yield return describe.Transient<IValueProviderFactory, RouteValueValueProviderFactory>();
            yield return describe.Transient<IValueProviderFactory, QueryStringValueProviderFactory>();
            yield return describe.Transient<IValueProviderFactory, FormValueProviderFactory>();

            yield return describe.Transient<IInputFormatter, JsonInputFormatter>();
            yield return describe.Transient<IInputFormatterProvider, TempInputFormatterProvider>();

            yield return describe.Transient<IModelBindersProvider, DefaultModelBindersProvider>();
            yield return describe.Transient<ICompositeModelBinder, CompositeModelBinder>();

            yield return describe.Transient<INestedProvider<FilterProviderContext>, DefaultFilterProvider>();

            yield return describe.Transient<IModelValidatorProvider, DataAnnotationsModelValidatorProvider>();
            yield return describe.Transient<IModelValidatorProvider, DataMemberModelValidatorProvider>();

            yield return describe.Scoped<IUrlHelper, UrlHelper>();

            yield return describe.Transient<IViewComponentSelector, DefaultViewComponentSelector>();
            yield return describe.Transient<IViewComponentInvokerFactory, DefaultViewComponentInvokerFactory>();
            yield return describe.Transient<INestedProvider<ViewComponentInvokerProviderContext>,
                DefaultViewComponentInvokerProvider>();
            yield return describe.Transient<IViewComponentHelper, DefaultViewComponentHelper>();

            yield return describe.Transient<IAuthorizationService, DefaultAuthorizationService>();
            yield return describe.Singleton<IClaimUidExtractor, DefaultClaimUidExtractor>();
            yield return describe.Singleton<AntiForgery, AntiForgery>();
            yield return describe.Singleton<IAntiForgeryAdditionalDataProvider,
                DefaultAntiForgeryAdditionalDataProvider>();

            yield return
               describe.Describe(
                   typeof(INestedProviderManager<>),
                   typeof(NestedProviderManager<>),
                   implementationInstance: null,
                   lifecycle: LifecycleKind.Transient);

            yield return
                describe.Describe(
                    typeof(INestedProviderManagerAsync<>),
                    typeof(NestedProviderManagerAsync<>),
                    implementationInstance: null,
                    lifecycle: LifecycleKind.Transient);

            yield return describe.Transient<IHtmlHelper, HtmlHelper>();
            yield return
                describe.Describe(
                    typeof(IHtmlHelper<>),
                    typeof(HtmlHelper<>),
                    implementationInstance: null,
                    lifecycle: LifecycleKind.Transient);
        }
    }
}
