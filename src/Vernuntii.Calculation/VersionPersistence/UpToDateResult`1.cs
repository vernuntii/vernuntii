using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionPersistence;

internal class UpToDateResult<TReason>
{
    public static readonly UpToDateResult<TReason> UpToDate = new UpToDateResult<TReason>(isUpTodate: true, reason: default);

    public static UpToDateResult<TReason> NotUpToDate(TReason reason) =>
        new UpToDateResult<TReason>(isUpTodate: false, reason);

    public virtual TReason? Reason { get; }

    [MemberNotNullWhen(false, nameof(Reason))]
    public virtual bool IsUpTodate { get; }

    protected UpToDateResult(bool isUpTodate, TReason? reason)
    {
        IsUpTodate = isUpTodate;
        Reason = reason;
    }
}
