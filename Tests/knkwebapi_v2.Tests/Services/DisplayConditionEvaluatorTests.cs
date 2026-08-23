using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using knkwebapi_v2.Enums;
using knkwebapi_v2.Models;
using knkwebapi_v2.Services;
using Xunit;

namespace knkwebapi_v2.Tests.Services
{
    /// <summary>
    /// Server-side enforcement of display conditions: values belonging to a hidden step or field
    /// must never survive into the payload used to create an entity.
    /// </summary>
    public class DisplayConditionEvaluatorTests
    {
        private static readonly Guid GateTypeGuid = Guid.NewGuid();
        private static readonly Guid WidthGuid = Guid.NewGuid();
        private static readonly Guid HingeGuid = Guid.NewGuid();
        private static readonly Guid NameGuid = Guid.NewGuid();

        private readonly DisplayConditionEvaluator _evaluator = new();

        private static FormField Field(Guid guid, string name) => new()
        {
            FieldGuid = guid,
            FieldName = name,
            Label = name,
            FieldType = FieldType.String
        };

        private static FormStep Step(string name, params FormField[] fields) => new()
        {
            StepGuid = Guid.NewGuid(),
            StepName = name,
            Fields = fields.ToList(),
            FieldOrderJson = JsonSerializer.Serialize(fields.Select(f => f.FieldGuid.ToString()))
        };

        private static DisplayConditionGroup Group(params DisplayCondition[] conditions) => new()
        {
            InnerLogic = DisplayConditionLogic.And,
            CombineWithPreviousLogic = DisplayConditionLogic.Or,
            IsActive = true,
            Conditions = conditions.ToList()
        };

        private static DisplayCondition Condition(Guid sourceGuid, ConditionOperator op, string valueJson, int order = 0) => new()
        {
            SourceFormFieldGuid = sourceGuid,
            Operator = op,
            ValueJson = valueJson,
            Order = order
        };

        private static FormConfiguration BuildConfig(out FormStep rotationStep)
        {
            var general = Step("General", Field(GateTypeGuid, "gateType"), Field(WidthGuid, "geometryWidth"));
            rotationStep = Step("Rotation", Field(HingeGuid, "hingeAxis"));
            var always = Step("Always", Field(NameGuid, "name"));

            rotationStep.DisplayConditionGroups.Add(
                Group(Condition(GateTypeGuid, ConditionOperator.Equals, "\"DRAWBRIDGE\"")));

            var steps = new List<FormStep> { general, rotationStep, always };

            return new FormConfiguration
            {
                EntityTypeName = "GateStructure",
                Name = "Gate",
                Steps = steps,
                StepOrderJson = JsonSerializer.Serialize(steps.Select(s => s.StepGuid.ToString()))
            };
        }

        [Fact]
        public void FilterVisibleValues_DropsValuesOfHiddenStep()
        {
            var config = BuildConfig(out _);
            var submitted = new Dictionary<string, object?>
            {
                ["gateType"] = "SLIDING",
                ["hingeAxis"] = "north",
                ["name"] = "Main gate"
            };

            var result = _evaluator.FilterVisibleValues(config, submitted);

            result.Should().ContainKey("gateType");
            result.Should().ContainKey("name");
            result.Should().NotContainKey("hingeAxis");
        }

        [Fact]
        public void FilterVisibleValues_KeepsValuesWhenConditionIsMet()
        {
            var config = BuildConfig(out _);
            var submitted = new Dictionary<string, object?>
            {
                ["gateType"] = "DRAWBRIDGE",
                ["hingeAxis"] = "north"
            };

            var result = _evaluator.FilterVisibleValues(config, submitted);

            result["hingeAxis"].Should().Be("north");
        }

        [Fact]
        public void FilterVisibleValues_DropsHiddenFieldInsideVisibleStep()
        {
            var config = BuildConfig(out _);
            config.Steps[2].Fields[0].DisplayConditionGroups.Add(
                Group(Condition(GateTypeGuid, ConditionOperator.Equals, "\"TRAP\"")));

            var result = _evaluator.FilterVisibleValues(config, new Dictionary<string, object?>
            {
                ["gateType"] = "SLIDING",
                ["name"] = "Main gate"
            });

            result.Should().NotContainKey("name");
        }

        [Theory]
        [InlineData(5, false)]
        [InlineData(6, true)]
        [InlineData(9, true)]
        public void FilterVisibleValues_HonoursGreaterOrEqual(int width, bool expectVisible)
        {
            var config = BuildConfig(out var rotationStep);
            rotationStep.DisplayConditionGroups.Clear();
            rotationStep.DisplayConditionGroups.Add(
                Group(Condition(WidthGuid, ConditionOperator.GreaterOrEqual, "6")));

            var result = _evaluator.FilterVisibleValues(config, new Dictionary<string, object?>
            {
                ["geometryWidth"] = width,
                ["hingeAxis"] = "north"
            });

            result.ContainsKey("hingeAxis").Should().Be(expectVisible);
        }

        [Theory]
        [InlineData("TRAP", true)]
        [InlineData("DOUBLE_DOORS", false)]
        public void FilterVisibleValues_HonoursInOperator(string gateType, bool expectVisible)
        {
            var config = BuildConfig(out var rotationStep);
            rotationStep.DisplayConditionGroups.Clear();
            rotationStep.DisplayConditionGroups.Add(
                Group(Condition(GateTypeGuid, ConditionOperator.In, "[\"SLIDING\",\"TRAP\",\"DRAWBRIDGE\"]")));

            var result = _evaluator.FilterVisibleValues(config, new Dictionary<string, object?>
            {
                ["gateType"] = gateType,
                ["hingeAxis"] = "north"
            });

            result.ContainsKey("hingeAxis").Should().Be(expectVisible);
        }

        [Fact]
        public void FilterVisibleValues_RequiresAllConditionsWhenLogicIsAnd()
        {
            var config = BuildConfig(out var rotationStep);
            rotationStep.DisplayConditionGroups.Clear();
            rotationStep.DisplayConditionGroups.Add(Group(
                Condition(GateTypeGuid, ConditionOperator.Equals, "\"DRAWBRIDGE\""),
                Condition(WidthGuid, ConditionOperator.GreaterOrEqual, "6", 1)));

            var partial = _evaluator.FilterVisibleValues(config, new Dictionary<string, object?>
            {
                ["gateType"] = "DRAWBRIDGE",
                ["geometryWidth"] = 4,
                ["hingeAxis"] = "north"
            });

            var full = _evaluator.FilterVisibleValues(config, new Dictionary<string, object?>
            {
                ["gateType"] = "DRAWBRIDGE",
                ["geometryWidth"] = 8,
                ["hingeAxis"] = "north"
            });

            partial.Should().NotContainKey("hingeAxis");
            full.Should().ContainKey("hingeAxis");
        }

        [Fact]
        public void FilterVisibleValues_CombinesGroupsWithOr()
        {
            var config = BuildConfig(out var rotationStep);
            rotationStep.DisplayConditionGroups.Clear();
            rotationStep.DisplayConditionGroups.Add(Group(
                Condition(GateTypeGuid, ConditionOperator.Equals, "\"DRAWBRIDGE\"")));
            var second = Group(Condition(GateTypeGuid, ConditionOperator.Equals, "\"DOUBLE_DOORS\""));
            second.Order = 1;
            second.CombineWithPreviousLogic = DisplayConditionLogic.Or;
            rotationStep.DisplayConditionGroups.Add(second);

            var result = _evaluator.FilterVisibleValues(config, new Dictionary<string, object?>
            {
                ["gateType"] = "DOUBLE_DOORS",
                ["hingeAxis"] = "north"
            });

            result.Should().ContainKey("hingeAxis");
        }

        [Fact]
        public void FilterVisibleValues_TargetWithoutGroupsIsAlwaysVisible()
        {
            var config = BuildConfig(out _);

            var result = _evaluator.FilterVisibleValues(config, new Dictionary<string, object?>
            {
                ["gateType"] = "SLIDING",
                ["name"] = "Main gate"
            });

            result["name"].Should().Be("Main gate");
        }

        [Fact]
        public void FilterVisibleValues_InactiveGroupIsIgnored()
        {
            var config = BuildConfig(out var rotationStep);
            rotationStep.DisplayConditionGroups[0].IsActive = false;

            var result = _evaluator.FilterVisibleValues(config, new Dictionary<string, object?>
            {
                ["gateType"] = "SLIDING",
                ["hingeAxis"] = "north"
            });

            result.Should().ContainKey("hingeAxis");
        }
    }
}
