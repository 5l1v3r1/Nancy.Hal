﻿namespace Nancy.Hal.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    using Nancy.Extensions;

    public class HalJsonTypeConfiguration
    {
        private readonly object syncRoot = new object();

        private readonly List<Func<object, NancyContext, Link>> links = new List<Func<object, NancyContext, Link>>();

        private readonly Dictionary<PropertyInfo, EmbeddedResource> embedded = new Dictionary<PropertyInfo, EmbeddedResource>();

        internal IReadOnlyList<Func<object, NancyContext, Link>> Links { get { return this.links; } }

        internal IReadOnlyDictionary<PropertyInfo, EmbeddedResource> Embedded { get { return this.embedded; } }

        public HalJsonTypeConfiguration Link(Link link)
        {
            lock (syncRoot)
            {
                links.Add((o, ctx) => link);
            }

            return this;
        }

        public HalJsonTypeConfiguration Link(string rel, string href)
        {
            lock (syncRoot)
            {
                links.Add((o, ctx) => new Link(rel, href));
            }

            return this;
        }

        public HalJsonTypeConfiguration Link(Func<object, NancyContext, Link> linkGetter)
        {
            lock (syncRoot)
            {
                links.Add(linkGetter);
            }

            return this;
        }

        internal HalJsonTypeConfiguration Embed(EmbeddedResource embed)
        {
            lock (syncRoot)
            {
                embedded.Add(embed.PropertyInfo, embed);
            }
            return this;
        }
    }

    public class HalJsonTypeConfiguration<T> : HalJsonTypeConfiguration
    {
        public HalJsonTypeConfiguration<T> Link(Func<T, Link> linkGetter)
        {
            this.Link((o, ctx) => linkGetter(o));
            return this;
        }

        public HalJsonTypeConfiguration<T> Link(Func<T, NancyContext, Link> linkGetter)
        {
            base.Link((o, ctx) => linkGetter((T)o, ctx));
            return this;
        }

        public HalJsonTypeConfiguration<T> Link(Func<T, Link> linkGetter, Func<T, bool> predicate)
        {
            base.Link(
                (o, ctx) =>
                {
                    var model = (T)o;
                    if (predicate(model))
                        return linkGetter(model);

                    return null;
                });
            return this;
        }

        public HalJsonTypeConfiguration<T> Link(Func<T, Link> linkGetter, Func<T, NancyContext, bool> predicate)
        {
            base.Link(
                (o, ctx) =>
                {
                    var model = (T)o;
                    if (predicate(model, ctx))
                        return linkGetter(model);
                    return null;
                });
            return this;
        }

        public HalJsonTypeConfiguration<T> Embed<TEmbedded>(Expression<Func<T, TEmbedded>> property)
        {
            var propertyInfo = Extensions.ExtractProperty(property);
            var getter = Extensions.CreateDelegate<Func<T, TEmbedded>>(propertyInfo.GetMethod, null);
            this.Embed(new EmbeddedResource<T, TEmbedded>(propertyInfo.Name.ToCamelCase(), propertyInfo, getter));
            return this;
        }

        public HalJsonTypeConfiguration<T> Embed<TEmbedded>(string rel, Expression<Func<T, TEmbedded>> property)
        {
            var propertyInfo = Extensions.ExtractProperty(property);
            var getter = Extensions.CreateDelegate<Func<T, TEmbedded>>(propertyInfo.GetMethod, null);
            this.Embed(new EmbeddedResource<T, TEmbedded>(rel, propertyInfo, getter));
            return this;
        }
    }
}