﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using dotless.Core.Parser.Functions;
using dotless.Core.Parser.Infrastructure;
using dotless.Core.Parser.Infrastructure.Nodes;
using dotless.Core.Parser.Tree;
using dotless.Core.Utils;

namespace dotless.NamedThemes
{
    public class GetThemeColorFunction : Function
    {
        protected override Node Evaluate(Env env)
        {
            Guard.ExpectNumArguments(2, Arguments.Count(), this, Location);
            Guard.ExpectNode<Keyword>(Arguments[0], this, Arguments[0].Location);
            Guard.ExpectNode<Keyword>(Arguments[1], this, Arguments[0].Location);

            var themeName = Arguments[0] as Keyword;
            var colorName = Arguments[1] as Keyword;

            var themeBaseUrl = ConfigurationManager.AppSettings["dotless.NamedThemes:ThemeBaseUrl"];
            var themeBasePath =  HttpContext.Current.Server.MapPath(themeBaseUrl);
            var themeBaseFile = Path.Combine(themeBasePath, themeName + ".less");

            Ruleset rules = GetCachedRuleset(themeBaseFile);

            var rule = rules.Rules
                .OfType<Rule>()
                .SingleOrDefault(a => a.Name == "@" + colorName.Value);

            if (rule == null)
            {
                return null;
            }

            return rule.Value;
        }

        private Ruleset GetCachedRuleset(string themeBaseFile)
        {
            var cacheKey = "dotless.namedtheme.basefile." + themeBaseFile;
            var cache = HttpContext.Current.Cache;

            var ruleset = cache[cacheKey] as Ruleset;
            if (ruleset == null)
            {

                var themeFileContent = File.ReadAllText(themeBaseFile);

                var parser = new dotless.Core.Parser.Parser();
                ruleset = parser.Parse(themeFileContent, themeBaseFile);

                cache.Insert(cacheKey, ruleset, new CacheDependency(themeBaseFile));
            }

            return ruleset;
        }

        
    }
}
