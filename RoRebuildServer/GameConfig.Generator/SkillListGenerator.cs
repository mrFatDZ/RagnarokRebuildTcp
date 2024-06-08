﻿using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Tomlyn;

namespace GameConfig.Generator
{
    /// <summary>
    /// Generates the CharacterSkill enum type directly from Skills.toml.
    /// </summary>
    [Generator]
    internal class SkillListGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            Debug.WriteLine("Execute code generator");
            var myFiles = context.AdditionalFiles.Where(at => at.Path.EndsWith("Skills.toml"));
            foreach (var file in myFiles)
            {
                var text = file.GetText().ToString();
                var table = Toml.ToModel(text);

                var srcOut = new StringBuilder();
                srcOut.AppendLine("namespace RebuildSharedData.Enum;");
                srcOut.AppendLine("public enum CharacterSkill : byte");
                srcOut.AppendLine("{");
                srcOut.AppendLine("\tNone,");
                foreach (var obj in table)
                {
                    srcOut.AppendLine($"\t{obj.Key},");
                }

                srcOut.AppendLine("}");

                //context.AddSource($"ClientSkill.g.cs", srcOut.ToString());
                context.AddSource($"ClientSkill.g.cs", SourceText.From(srcOut.ToString(), Encoding.UTF8));
            }
        }
    }
}

