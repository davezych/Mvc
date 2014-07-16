// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class MvcRazorHost : RazorEngineHost, IMvcRazorHost
    {
        private const string ViewNamespace = "ASP";

        private static readonly string[] _defaultNamespaces = new[] 
        { 
            "System",
            "System.Linq",
            "System.Collections.Generic",
            "Microsoft.AspNet.Mvc",
            "Microsoft.AspNet.Mvc.Razor",
            "Microsoft.AspNet.Mvc.Rendering"
        };

        private readonly MvcRazorHostOptions _hostOptions;

        // CodeGenerationContext.DefaultBaseClass is set to MyBaseType<dynamic>. 
        // This field holds the type name without the generic decoration (MyBaseType)
        private readonly string _baseType;

        private IRazorCodeBuilderProvider _codeBuilderProvider;
        private IRazorCodeParserProvider _codeParserProvider;

        // TODO: Change parser and builder to be interfaces? Would have to happen in Razor lib
        public MvcRazorHost(IRazorBaseTypeResolver baseTypeResolver,
                            IRazorCodeBuilderProvider codeBuilderProvider,
                            IRazorCodeParserProvider parserProvider,
                            ITagHelperProviderContext generatedTagHelperContext)
            : base(new CSharpRazorCodeLanguage())
        {
            // TODO: this needs to flow from the application rather than being initialized here.
            // Tracked by #774
            _hostOptions = new MvcRazorHostOptions();
            _codeBuilderProvider = codeBuilderProvider;
            _codeParserProvider = parserProvider;

            var baseType = baseTypeResolver.Resolve();

            _baseType = baseType;
            DefaultBaseClass = baseType + '<' + _hostOptions.DefaultModel + '>';
            GeneratedClassContext = new MvcGeneratedClassContext();

            GeneratedTagHelperContext = generatedTagHelperContext;

            foreach (var ns in _defaultNamespaces)
            {
                NamespaceImports.Add(ns);
            }
        }

        public GeneratorResults GenerateCode(string rootRelativePath, Stream inputStream)
        {
            var className = ParserHelpers.SanitizeClassName(rootRelativePath);
            using (var reader = new StreamReader(inputStream))
            {
                var engine = new RazorTemplateEngine(this);
                return engine.GenerateCode(reader, className, ViewNamespace, rootRelativePath);
            }
        }

        public override ParserBase DecorateCodeParser(ParserBase incomingCodeParser)
        {
            return _codeParserProvider.GetCodeParser(incomingCodeParser, _baseType);
        }

        public override CodeBuilder DecorateCodeBuilder(CodeBuilder incomingBuilder, CodeGeneratorContext context)
        {
            return _codeBuilderProvider.GetCodeBuilder(incomingBuilder, _hostOptions, context);
        }
    }
}
