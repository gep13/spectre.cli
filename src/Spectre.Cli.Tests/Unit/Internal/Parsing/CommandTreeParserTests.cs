﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Shouldly;
using Spectre.Cli.Internal.Configuration;
using Spectre.Cli.Internal.Modelling;
using Spectre.Cli.Internal.Parsing;
using Spectre.Cli.Tests.Data;
using Spectre.Cli.Tests.Data.Settings;
using Xunit;

namespace Spectre.Cli.Tests.Unit.Internal.Parsing
{
    public sealed class CommandTreeParserTests
    {
        [Fact]
        public void Should_Capture_Remaining_Arguments()
        {
            // Given, When
            var (_, remaining) = Fixture.Parse(new[] { "dog", "--", "--foo", "-bar", "\"baz\"", "qux" }, config =>
            {
                config.AddCommand<DogCommand>("dog");
            });

            // Then
            remaining.Count.ShouldBe(4);
            remaining[0].ShouldBe("--foo");
            remaining[1].ShouldBe("-bar");
            remaining[2].ShouldBe("\"baz\"");
            remaining[3].ShouldBe("qux");
        }

        /// <summary>
        /// https://github.com/spectresystems/spectre.cli/wiki/Test-cases#test-case-1
        /// </summary>
        [Theory]
        [EmbeddedResourceData("Spectre.Cli.Tests/Data/Resources/Parsing/case1.xml")]
        public void Should_Parse_Correct_Tree_For_Case_1(string expected)
        {
            // Given, When
            var result = Fixture.Serialize(
                new[] { "animal", "--alive", "mammal", "--name", "Rufus", "dog", "12", "--good-boy" },
                config =>
                {
                    config.AddBranch<AnimalSettings>("animal", animal =>
                    {
                        animal.AddBranch<MammalSettings>("mammal", mammal =>
                        {
                            mammal.AddCommand<DogCommand>("dog");
                            mammal.AddCommand<HorseCommand>("horse");
                        });
                    });
                });

            // Then
            result.ShouldBe(expected);
        }

        /// <summary>
        /// https://github.com/spectresystems/spectre.cli/wiki/Test-cases#test-case-2
        /// </summary>
        [Theory]
        [EmbeddedResourceData("Spectre.Cli.Tests/Data/Resources/Parsing/case2.xml")]
        public void Should_Parse_Correct_Tree_For_Case_2(string expected)
        {
            // Given, When
            var result = Fixture.Serialize(
                new[] { "dog", "12", "4", "--good-boy", "--name", "Rufus", "--alive" },
                config =>
                {
                    config.AddCommand<DogCommand>("dog");
                });

            // Then
            result.ShouldBe(expected);
        }

        /// <summary>
        /// https://github.com/spectresystems/spectre.cli/wiki/Test-cases#test-case-3
        /// </summary>
        [Theory]
        [EmbeddedResourceData("Spectre.Cli.Tests/Data/Resources/Parsing/case3.xml")]
        public void Should_Parse_Correct_Tree_For_Case_3(string expected)
        {
            // Given, When
            var result = Fixture.Serialize(
                new[] { "animal", "dog", "12", "--good-boy", "--name", "Rufus" },
                config =>
            {
                config.AddBranch<AnimalSettings>("animal", animal =>
                {
                    animal.AddCommand<DogCommand>("dog");
                    animal.AddCommand<HorseCommand>("horse");
                });
            });

            // Then
            result.ShouldBe(expected);
        }

        /// <summary>
        /// https://github.com/spectresystems/spectre.cli/wiki/Test-cases#test-case-4
        /// </summary>
        [Theory]
        [EmbeddedResourceData("Spectre.Cli.Tests/Data/Resources/Parsing/case4.xml")]
        public void Should_Parse_Correct_Tree_For_Case_4(string expected)
        {
            // Given, When
            var result = Fixture.Serialize(
                new[] { "animal", "4", "dog", "12", "--good-boy", "--name", "Rufus" },
                config =>
                {
                    config.AddBranch<AnimalSettings>("animal", animal =>
                    {
                        animal.AddCommand<DogCommand>("dog");
                    });
                });

            // Then
            result.ShouldBe(expected);
        }

        private static class Fixture
        {
            public static (CommandTree, IReadOnlyList<string> remaining) Parse(IEnumerable<string> args, Action<Configurator> func)
            {
                var configurator = new Configurator(null);
                func(configurator);

                var model = CommandModelBuilder.Build(configurator);
                return new CommandTreeParser(model).Parse(args);
            }

            public static string Serialize(IEnumerable<string> args, Action<Configurator> func)
            {
                var (tree, _) = Parse(args, func);

                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\n",
                    OmitXmlDeclaration = false
                };

                using (var buffer = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(buffer, settings))
                {
                    CommandTreeSerializer.Serialize(tree).WriteTo(xmlWriter);
                    xmlWriter.Flush();
                    return buffer.GetStringBuilder().ToString().NormalizeLineEndings();
                }
            }
        }
    }
}
