﻿using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_an_empty_library_project
	{
		ITestOutputHelper output;

		public given_an_empty_library_project(ITestOutputHelper output)
		{
			this.output = output;
            Builder.BuildScenario(nameof(given_an_empty_library_project), target: "Restore")
                .AssertSuccess(output);
        }

        [Fact]
		public void when_getting_package_contents_then_includes_output_assembly()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project));

			result.AssertSuccess(output);
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Extension = ".dll",
				Kind = "Lib"
			}));
		}

		[Fact]
		public void when_include_outputs_in_package_is_false_then_does_not_include_main_assembly()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new
			{
				PackBuildOutput = false,
			});

			result.AssertSuccess(output);
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Extension = ".dll",
				Kind = "Lib"
			}));
		}

		[Fact]
		public void when_getting_package_contents_then_includes_symbols()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new { Configuration = "Debug" });

			result.AssertSuccess(output);
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Extension = ".pdb",
			}));
		}

		[Fact]
		public void when_include_symbols_in_package_is_true_but_include_outputs_is_false_then_does_not_include_symbols()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new
			{
				PackBuildOutput = false,
				PackSymbols = true,
			});

			result.AssertSuccess(output);
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Extension = ".pdb",
			}));
		}

		[Fact]
		public void when_include_symbols_in_package_is_false_then_does_not_include_symbols()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new
			{
				PackSymbols = false,
			});

			result.AssertSuccess(output);
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Extension = ".pdb",
			}));
		}

		[Fact]
		public void when_getting_package_contents_then_includes_xmldoc()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project));

			Assert.Equal(TargetResultCode.Success, result.ResultCode);
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Extension = ".xml",
				Kind = PackageItemKind.Lib,
			}));
		}

		[Fact]
		public void when_include_output_in_package_is_false_then_does_not_include_xmldoc()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new
			{
				PackBuildOutput = false,
			});

			result.AssertSuccess(output);
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Extension = ".xml",
				Kind = PackageItemKind.Lib,
			}));
		}

		[Fact]
		public void when_getting_package_contents_then_annotates_items_with_package_id()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new { PackageId = "Foo" }, output: output);

			result.AssertSuccess(output);
			Assert.All(result.Items, item => Assert.Equal("Foo", item.GetMetadata("PackageId")));
		}

		[Fact]
		public void when_getting_package_contents_then_includes_framework_reference()
		{
            Builder.BuildScenario(nameof(given_an_empty_library_project), new
            {
                TargetFramework = "net472"
            },target: "Restore").AssertSuccess(output);

            var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new 
            { 
                PackageId = "Foo",
                TargetFramework = "net472"
            }, output: output);

			result.AssertSuccess(output);
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Identity = "System.Core",
				Kind = PackageItemKind.FrameworkReference,
			}));
		}

		[Fact]
		public void when_include_framework_references_in_package_is_false_then_does_not_include_framework_reference()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new
			{
				PackFrameworkReferences = false,
				PackageId = "Foo",
			}, output: output);

			result.AssertSuccess(output);
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Identity = "System.Core",
				Kind = PackageItemKind.FrameworkReference,
			}));
		}

        [Fact]
        public void when_getting_metadata_then_adds_repository_info()
        {
            var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new 
            { 
                PackageId = "Foo",
                PublishRepositoryUrl = "true",
            }, target: "GetPackageMetadata", output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Equal("git", metadata.GetMetadata("RepositoryType"));
            Assert.Equal("https://github.com/kzu/NuGetizer", metadata.GetMetadata("RepositoryUrl"));
            Assert.NotEmpty(metadata.GetMetadata("RepositoryCommit"));
        }

        [Fact]
        public void when_getting_metadata_with_no_explicit_publish_repo_url_then_does_not_expose_it()
        {
            var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new
            {
                PackageId = "Foo",
            }, target: "GetPackageMetadata", output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Empty(metadata.GetMetadata("RepositoryUrl"));
            Assert.NotEmpty(metadata.GetMetadata("RepositoryCommit"));
        }

        [Fact]
        public void when_updating_package_item_metadata_then_updates_metadata()
        {
            var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new
            {
                PackageId = "Foo",
                AdvancedCustomization = "true",
            }, target: "GetPackageMetadata", output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Equal("ItemDescription", metadata.GetMetadata("Description"));
        }

        [Fact]
        public void when_updating_package_metadata_property_in_target_then_updates_metadata()
        {
            var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new
            {
                PackageId = "Foo",
            }, target: "GetPackageMetadata", output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Equal("PropertyDescription", metadata.GetMetadata("Description"));
        }

        [Fact]
        public void when_setting_metadata_property_then_updates_metadata()
        {
            var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new
            {
                PackageId = "Foo",
                Title = "MyPackage",
            }, target: "GetPackageMetadata", output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Equal("MyPackage", metadata.GetMetadata("Title"));
        }
    }
}
