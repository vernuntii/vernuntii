/* The MIT License (MIT)

Copyright (c) 2021 NServiceBus Ltd, GitTools and contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
using System.Text;
using GitTools.Testing.Internal;

namespace GitTools.Testing;

/// <summary>
/// Creates an abstraction over a PlantUML Sequence diagram to draw a sequence diagram of a git repository being created
/// </summary>
public class SequenceDiagram
{
    private readonly Dictionary<string, string> participants = new();
    private readonly StringBuilder diagramBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:SequenceDiagram"/> class.
    /// </summary>
    public SequenceDiagram()
    {
        this.diagramBuilder = new StringBuilder();
        this.diagramBuilder.AppendLine("@startuml");
    }

    /// <summary>
    /// Activates a branch/participant in the sequence diagram
    /// </summary>
    public void Activate(string branch) => this.diagramBuilder.AppendLineFormat("activate {0}", GetParticipant(branch));

    /// <summary>
    /// Destroys a branch/participant in the sequence diagram
    /// </summary>
    public void Destroy(string branch) => this.diagramBuilder.AppendLineFormat("destroy {0}", GetParticipant(branch));

    /// <summary>
    /// Creates a participant in the sequence diagram
    /// </summary>
    public void Participant(string participant, string? @as = null)
    {
        this.participants.Add(participant, @as ?? participant);
        if (@as == null)
            this.diagramBuilder.AppendLineFormat("participant {0}", participant);
        else
            this.diagramBuilder.AppendLineFormat("participant \"{0}\" as {1}", participant, @as);
    }

    /// <summary>
    /// Appends a divider with specified text to the sequence diagram
    /// </summary>
    public void Divider(string text) => this.diagramBuilder.AppendLineFormat("== {0} ==", text);

    /// <summary>
    /// Appends a note over one or many participants to the sequence diagram
    /// </summary>
    public void NoteOver(string noteText, string startParticipant, string? endParticipant = null, string? prefix = null, string? color = null) =>
        this.diagramBuilder.AppendLineFormat(
            prefix + @"note over {0}{1}{2}
  {3}
end note",
            GetParticipant(startParticipant),
            endParticipant == null ? null : ", " + GetParticipant(endParticipant),
            color == null ? null : " " + color,
            noteText.Replace("\n", "\n  "));

    /// <summary>
    /// Appends applying a tag to the specified branch/participant to the sequence diagram
    /// </summary>
    public void ApplyTag(string tag, string toBranch) => this.diagramBuilder.AppendLineFormat("{0} -> {0}: tag {1}", GetParticipant(toBranch), tag);

    /// <summary>
    /// Appends branching from a branch to another branch, @as can override the participant name
    /// </summary>
    public void BranchTo(string branchName, string currentName, string? @as)
    {
        if (!this.participants.ContainsKey(branchName))
        {
            this.diagramBuilder.Append("create ");
            Participant(branchName, @as);
        }

        this.diagramBuilder.AppendLineFormat(
            "{0} -> {1}: branch from {2}",
            GetParticipant(currentName),
            GetParticipant(branchName), currentName);
    }

    /// <summary>
    /// Appends branching from a tag to a specified branch to the sequence diagram
    /// </summary>
    public void BranchToFromTag(string branchName, string fromTag, string onBranch, string? @as)
    {
        if (!this.participants.ContainsKey(branchName))
        {
            this.diagramBuilder.Append("create ");
            Participant(branchName, @as);
        }

        this.diagramBuilder.AppendLineFormat("{0} -> {1}: branch from tag ({2})", GetParticipant(onBranch), GetParticipant(branchName), fromTag);
    }

    /// <summary>
    /// Appends a commit on the target participant/branch to the sequence diagram
    /// </summary>
    public void MakeACommit(string toParticipant) => this.diagramBuilder.AppendLineFormat("{0} -> {0}: commit", GetParticipant(toParticipant));

    /// <summary>
    /// Append a merge to the sequence diagram
    /// </summary>
    public void Merge(string from, string to) => this.diagramBuilder.AppendLineFormat("{0} -> {1}: merge", GetParticipant(from), GetParticipant(to));

    private string GetParticipant(string branch) => this.participants.ContainsKey(branch) ? this.participants[branch] : branch;

    /// <summary>
    /// Ends the sequence diagram
    /// </summary>
    public void End() => this.diagramBuilder.AppendLine("@enduml");

    /// <summary>
    /// returns the plantUML representation of the Sequence Diagram
    /// </summary>
    public string GetDiagram() => this.diagramBuilder.ToString();
}
