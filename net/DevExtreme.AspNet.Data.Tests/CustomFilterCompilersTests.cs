﻿using DevExtreme.AspNet.Data.Helpers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Xunit;

namespace DevExtreme.AspNet.Data.Tests {

    public class CustomFilterCompilersTests {
        static readonly JsonSerializerOptions TESTS_DEFAULT_SERIALIZER_OPTIONS = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
            Converters = { new ListConverter() }
        };

        [Fact]
        public void OneToManyContains() {
            // https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/277#issuecomment-497249871

            try {
                CustomFilterCompilers.RegisterBinaryExpressionCompiler(info => {
                    if(info.DataItemExpression.Type == typeof(Category) && info.AccessorText == "Products" && info.Operation == "contains") {
                        var text = Convert.ToString(info.Value);

                        Expression<Func<Product, bool>> innerLambda = (p) => p.Name.ToLower().Contains(text);

                        return Expression.Call(
                            typeof(Enumerable), nameof(Enumerable.Any), new[] { typeof(Product) },
                            Expression.Property(info.DataItemExpression, "Products"), innerLambda
                        );
                    }

                    return null;
                });

                var source = new[] { new Category(), new Category() };
                source[0].Products.Add(new Product { Name = "Chai" });

                var filter = JsonSerializer.Deserialize<IList>(@"[ ""Products"", ""Contains"", ""ch"" ]", TESTS_DEFAULT_SERIALIZER_OPTIONS);

                var loadOptions = new SampleLoadOptions {
                    Filter = filter,
                    RequireTotalCount = true
                };

                var loadResult = DataSourceLoader.Load(source, loadOptions);
                Assert.Equal(1, loadResult.totalCount);
                Assert.Contains(loadOptions.ExpressionLog, line => line.Contains(".Products.Any(p => p.Name.ToLower().Contains("));
            } finally {
                CustomFilterCompilers.Binary.CompilerFuncs.Clear();
            }
        }

        [Fact]
        public void ArrayContainsWithNullGuard() {
            try {
                CustomFilterCompilers.RegisterBinaryExpressionCompiler(info => {
                    if(info.DataItemExpression.Type == typeof(Post) && info.AccessorText == "Tags" && info.Operation == "contains") {
                        var text = Convert.ToString(info.Value);

                        var tagsAccessor = Expression.Property(info.DataItemExpression, "Tags");

                        var containsCall = Expression.Call(
                            typeof(Enumerable), nameof(Enumerable.Contains), new[] { typeof(string) },
                            tagsAccessor, Expression.Constant(text) /*, Expression.Constant(StringComparer.InvariantCultureIgnoreCase)*/
                        );

                        return Expression.Condition(
                            Expression.MakeBinary(ExpressionType.NotEqual, tagsAccessor, Expression.Constant(null, typeof(string[]))),
                            containsCall,
                            Expression.Constant(false)
                        );
                    }

                    return null;
                });

                var source = new[] {
                    new Post { Tags = new[] { "news", "article" } },
                    new Post { Tags = new[] { "announcement" } }
                };

                var loadOptions = new SampleLoadOptions {
                    Filter = new[] { "Tags", "contains", "news" },
                    RequireTotalCount = true
                };

                var loadResult = DataSourceLoader.Load(source, loadOptions);
                Assert.Equal(1, loadResult.totalCount);
                Assert.Contains(loadOptions.ExpressionLog, line => line.Contains(@".Where(obj => IIF((obj.Tags != null), obj.Tags.Contains(""news""), False))"));
            } finally {
                CustomFilterCompilers.Binary.CompilerFuncs.Clear();
            }
        }

        [Fact]
        public void StringToLower() {
            try {
                var stringToLowerLog = new List<bool>();

                CustomFilterCompilers.RegisterBinaryExpressionCompiler(info => {
                    stringToLowerLog.Add(info.StringToLower);
                    return Expression.Constant(true);
                });

                foreach(var stringToLower in new[] { false, true }) {
                    DataSourceLoader.Load(new Product[0], new SampleLoadOptions {
                        Filter = new[] { "any", "any", "any" },
                        StringToLower = stringToLower
                    });
                }

                Assert.False(stringToLowerLog[0]);
                Assert.True(stringToLowerLog[1]);
            } finally {
                CustomFilterCompilers.Binary.CompilerFuncs.Clear();
            }
        }

        class Product {
            public string Name { get; set; }
        }

        class Category {
            public ICollection<Product> Products { get; } = new List<Product>();
        }

        class Post {
            public string[] Tags { get; set; }
        }

    }

}
