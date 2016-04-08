﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace DevExtreme.AspNet.Data {

    class DevExtremeGroup {
        public object key;
        public IList<object> items;
    }


    class GroupHelper<T> : ExpressionCompiler {
        IDictionary<string, Func<T, object>> _accessors = new Dictionary<string, Func<T, object>>();
        IEnumerable<T> _data;

        public GroupHelper(IEnumerable<T> data) {
            _data = data;
        }

        public IList<DevExtremeGroup> Group(params string[] selectors) {
            return Group(_data, selectors);
        }

        IList<DevExtremeGroup> Group(IEnumerable<T> data, IEnumerable<string> selectors) {
            var groups = Group(data, selectors.First());

            if(selectors.Count() > 1) {
                groups = groups
                    .Select(g => new DevExtremeGroup {
                        key = g.key,
                        items = Group(g.items.Cast<T>(), selectors.Skip(1))
                            .Cast<object>()
                            .ToArray()
                    })
                    .ToArray();
            }

            return groups;
        }


        IList<DevExtremeGroup> Group(IEnumerable<T> data, string selector) {
            var map = new Dictionary<object, DevExtremeGroup>();
            var groups = new List<DevExtremeGroup>();

            foreach(var item in data) {
                var key = GetMember(item, selector);
                if(!map.ContainsKey(key)) {
                    var group = new DevExtremeGroup {
                        key = key,
                        items = new List<object>()
                    };
                    map[key] = group;
                    groups.Add(group);
                }
                map[key].items.Add(item);
            }

            return groups;
        }


        object GetMember(T obj, string name) {
            if(!_accessors.ContainsKey(name)) {
                var param = CreateItemParam(typeof(T));

                _accessors[name] = Expression.Lambda<Func<T, object>>(
                    Expression.Convert(CompileAccessorExpression(param, name), typeof(Object)),
                    param
                ).Compile();
            }

            return _accessors[name](obj);
        }


    }

}