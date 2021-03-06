// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class GenericModelBinder : IModelBinder
    {
        private readonly ITypeActivator _activator;
        private readonly IServiceProvider _serviceProvider;

        public GenericModelBinder(IServiceProvider serviceProvider, ITypeActivator activator)
        {
            _serviceProvider = serviceProvider;
            _activator = activator;
        }

        public Task<bool> BindModelAsync(ModelBindingContext bindingContext)
        {
            var binderType = ResolveBinderType(bindingContext.ModelType);
            if (binderType != null)
            {
                var binder = (IModelBinder)_activator.CreateInstance(_serviceProvider, binderType);
                return binder.BindModelAsync(bindingContext);
            }

            return Task.FromResult(false);
        }

        private static Type ResolveBinderType(Type modelType)
        {
            return GetArrayBinder(modelType) ??
                   GetCollectionBinder(modelType) ??
                   GetDictionaryBinder(modelType) ??
                   GetKeyValuePairBinder(modelType);
        }

        private static Type GetArrayBinder(Type modelType)
        {
            if (modelType.IsArray)
            {
                var elementType = modelType.GetElementType();
                return typeof(ArrayModelBinder<>).MakeGenericType(elementType);
            }
            return null;
        }

        private static Type GetCollectionBinder(Type modelType)
        {
            return GetGenericBinderType(
                        typeof(ICollection<>),
                        typeof(List<>),
                        typeof(CollectionModelBinder<>),
                        modelType);
        }

        private static Type GetDictionaryBinder(Type modelType)
        {
            return GetGenericBinderType(
                        typeof(IDictionary<,>),
                        typeof(Dictionary<,>),
                        typeof(DictionaryModelBinder<,>),
                        modelType);
        }

        private static Type GetKeyValuePairBinder(Type modelType)
        {
            return ModelBindingHelper.GetPossibleBinderInstanceType(
                        closedModelType: modelType,
                        openModelType: typeof(KeyValuePair<,>),
                        openBinderType: typeof(KeyValuePairModelBinder<,>));
        }

        /// <remarks>
        /// Example: GetGenericBinder(typeof(IList<>), typeof(List<>), typeof(ListBinder<>), ...) means that the 
        /// ListBinder<T> type can update models that implement IList<T>, and if for some reason the existing model
        /// instance is not updatable the binder will create a List<T> object and bind to that instead. This method
        /// will return ListBinder<T> or null, depending on whether the type and updatability checks succeed.
        /// </remarks>
        private static Type GetGenericBinderType(Type supportedInterfaceType,
                                                 Type newInstanceType,
                                                 Type openBinderType,
                                                 Type modelType)
        {
            Contract.Assert(supportedInterfaceType != null);
            Contract.Assert(openBinderType != null);
            Contract.Assert(modelType != null);

            var modelTypeArguments = GetGenericBinderTypeArgs(supportedInterfaceType, modelType);

            if (modelTypeArguments == null)
            {
                return null;
            }

            var closedNewInstanceType = newInstanceType.MakeGenericType(modelTypeArguments);
            if (!modelType.GetTypeInfo().IsAssignableFrom(closedNewInstanceType.GetTypeInfo()))
            {
                return null;
            }

            return openBinderType.MakeGenericType(modelTypeArguments);
        }

        // Get the generic arguments for the binder, based on the model type. Or null if not compatible.
        private static Type[] GetGenericBinderTypeArgs(Type supportedInterfaceType, Type modelType)
        {
            var modelTypeInfo = modelType.GetTypeInfo();
            if (!modelTypeInfo.IsGenericType || modelTypeInfo.IsGenericTypeDefinition)
            {
                // not a closed generic type
                return null;
            }

            var modelTypeArguments = modelTypeInfo.GenericTypeArguments;
            if (modelTypeArguments.Length != supportedInterfaceType.GetTypeInfo().GenericTypeParameters.Length)
            {
                // wrong number of generic type arguments
                return null;
            }

            return modelTypeArguments;
        }
    }
}
