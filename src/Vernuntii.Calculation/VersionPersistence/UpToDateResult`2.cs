using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionPersistence;

internal class UpToDateResult<TSubject, TReason> : UpToDateResult<TReason>
{
    public new static UpToDateResult<TReason> UpToDate(TSubject subject) => 
        new UpToDateResult<TSubject, TReason>(subject, isUpTodate: true, reason: default);

    public static UpToDateResult<TReason> NotUpToDate(TSubject subject, TReason reason) =>
        new UpToDateResult<TSubject, TReason>(subject, isUpTodate: false, reason);

    public TSubject? Subject { get; }

    public override TReason? Reason =>
        base.Reason;

    [MemberNotNullWhen(true, nameof(Subject))]
    [MemberNotNullWhen(false, nameof(Reason))]
    public override bool IsUpTodate =>
        base.IsUpTodate;

    protected UpToDateResult(TSubject? subject, bool isUpTodate, TReason? reason)
        : base(isUpTodate, reason) =>
        Subject = subject;
}
