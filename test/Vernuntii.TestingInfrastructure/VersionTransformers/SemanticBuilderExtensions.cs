using Vernuntii.HeightConventions;
using Vernuntii.HeightConventions.Transformation;
using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers;

internal static class SemanticBuilderExtensions
{
    private static readonly NextHeightNumberTransformer s_nextPreReleaseHeightTransformer =
        new(
            new HeightConventionTransformer(
                HeightConvention.InPreReleaseAfterFirstDot,
                HeightPlaceholderParser.Default));

    public static SemanticVersionBuilder IncrementPreReleaseHeight(this SemanticVersionBuilder builder) =>
        s_nextPreReleaseHeightTransformer.TransformVersion(builder.ToVersion()).With();

    public static SemanticVersionBuilder IncrementMajor(this SemanticVersionBuilder builder) =>
        NextMajorVersionTransformer.Default.TransformVersion(builder.ToVersion()).With();

    public static SemanticVersionBuilder IncrementMinor(this SemanticVersionBuilder builder) =>
        NextMinorVersionTransformer.Default.TransformVersion(builder.ToVersion()).With();

    public static SemanticVersionBuilder IncrementPatch(this SemanticVersionBuilder builder) =>
        NextPatchVersionTransformer.Default.TransformVersion(builder.ToVersion()).With();

    public static SemanticVersionBuilder ToVersion(this SemanticVersionBuilder input, out SemanticVersionBuilder output) =>
        output = input.ToVersion().With;
}
