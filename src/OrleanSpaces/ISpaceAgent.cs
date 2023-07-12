﻿using OrleanSpaces.Observers;
using OrleanSpaces.Tuples;

namespace OrleanSpaces;

public interface ISpaceAgent
{
    /// <summary>
    /// Enables the <paramref name="observer"/> to subscribe to events that happen in the tuple space.
    /// </summary>
    /// <param name="observer">Any space observer.</param>
    /// <remarks><i>Method is idempotant.</i></remarks>
    /// <returns>An ID that can be used to <see cref="Unsubscribe"/>.</returns>
    Guid Subscribe(ISpaceObserver<SpaceTuple> observer);
    /// <summary>
    /// Removes the observer with the corresponding <paramref name="observerId"/>.
    /// </summary>
    /// <param name="observerId">The ID obtained from calling <see cref="Subscribe"/>.</param>
    /// <remarks><i>Method is idempotant.</i></remarks>
    void Unsubscribe(Guid observerId);

    /// <summary>
    /// Directly writes the <paramref name="tuple"/> in the space.
    /// </summary>
    /// <param name="tuple">Any <see cref="SpaceTuple"/> with non-zero length.</param>
    Task WriteAsync(SpaceTuple tuple);
    /// <summary>
    /// Indirectly writes a <see cref="SpaceTuple"/> in the space.
    /// <list type="number">
    /// <item><description>Executes the <paramref name="evaluation"/> function.</description></item>
    /// <item><description>Proceeds to write the resulting <see cref="SpaceTuple"/> in the space.</description></item>
    /// </list>
    /// </summary>
    /// <param name="evaluation">Any function that returns a <see cref="SpaceTuple"/> with non-zero length.</param>
    ValueTask EvaluateAsync(Func<Task<SpaceTuple>> evaluation);
    /// <summary>
    /// Reads a <see cref="SpaceTuple"/> that is potentially matched by the given <paramref name="template"/>.
    /// <list type="bullet">
    /// <item><description>If one such tuple exists, then a <u>copy</u> is returned thereby keeping the original in the space.</description></item>
    /// <item><description>Otherwise a <see cref="SpaceTuple"/> with zero length is returned to indicate a "no-match".</description></item>
    /// </list>
    /// </summary>
    /// <param name="template">A template that potentially matches a <see cref="SpaceTuple"/>.</param>
    /// <returns><see cref="SpaceTuple"/> (potentially one with zero length).</returns>
    ValueTask<SpaceTuple> PeekAsync(SpaceTemplate template);
    /// <summary>
    /// Reads a <see cref="SpaceTuple"/> that is potentially matched by the given <paramref name="template"/>.
    /// <list type="bullet">
    /// <item><description>If one such tuple exists, the <paramref name="callback"/> is invoked immediately.</description></item>
    /// <item><description>Otherwise the operation is remembered and the <paramref name="callback"/> will eventually be invoked as soon as a matching <see cref="SpaceTuple"/> is written in the space.</description></item>
    /// </list>
    /// </summary>
    /// <param name="template">A template that potentially matches a <see cref="SpaceTuple"/>.</param>
    /// <param name="callback">A callback function that will be executed, with the <see cref="SpaceTuple"/> as the argument.</param>
    /// <remarks><i>Same as with <see cref="PeekAsync(SpaceTemplate)"/>, the original tuple is <u>kept</u> in the space once <paramref name="callback"/> gets invoked.</i></remarks>
    ValueTask PeekAsync(SpaceTemplate template, Func<SpaceTuple, Task> callback);
    /// <summary>
    /// Reads a <see cref="SpaceTuple"/> that is potentially matched by the given <paramref name="template"/>.
    /// <list type="bullet">
    /// <item><description>If one such tuple exists, then the <u>original</u> is returned thereby removing it from the space.</description></item>
    /// <item><description>Otherwise a <see cref="SpaceTuple"/> with zero length is returned to indicate a "no-match".</description></item>
    /// </list>
    /// </summary>
    /// <param name="template">A template that potentially matches a <see cref="SpaceTuple"/>.</param>
    /// <returns><see cref="SpaceTuple"/> (potentially one with zero length)>.</returns>
    ValueTask<SpaceTuple> PopAsync(SpaceTemplate template);
    /// <summary>
    /// Reads a <see cref="SpaceTuple"/> that is potentially matched by the given <paramref name="template"/>.
    /// <list type="bullet">
    /// <item><description>If one such tuple exists, the <paramref name="callback"/> is invoked immediately.</description></item>
    /// <item><description>Otherwise the operation is remembered and the <paramref name="callback"/> will eventually be invoked as soon as a matching <see cref="SpaceTuple"/> is written in the space.</description></item>
    /// </list>
    /// </summary>
    /// <param name="template">A template that potentially matches a <see cref="SpaceTuple"/>.</param>
    /// <param name="callback">A callback function that will be executed, with the <see cref="SpaceTuple"/> as the argument.</param>
    /// <remarks><i>Same as with <see cref="PopAsync(SpaceTemplate)"/>, the original tuple is <u>removed</u> from the space once <paramref name="callback"/> gets invoked.</i></remarks>
    ValueTask PopAsync(SpaceTemplate template, Func<SpaceTuple, Task> callback);
    /// <summary>
    /// Reads multiple <see cref="SpaceTuple"/>'s that are potentially matched by the given <paramref name="template"/>.
    /// </summary>
    /// <param name="template">A template that potentially matches multiple <see cref="SpaceTuple"/>'s.</param>
    /// <remarks><i>Same as with <see cref="PeekAsync(SpaceTemplate)"/>, the original tuple's are <u>kept</u> in the space.</i></remarks>
    ValueTask<IEnumerable<SpaceTuple>> ScanAsync(SpaceTemplate template);
    /// <summary>
    /// Reads a stream of <see cref="SpaceTuple"/>'s as they are written in the tuple space.
    /// </summary>
    IAsyncEnumerable<SpaceTuple> ConsumeAsync();
    /// <summary>
    /// Returns the total number of <see cref="SpaceTuple"/>'s in the space. 
    /// </summary>
    ValueTask<int> CountAsync();
    /// <summary>
    /// Removes all <see cref="SpaceTuple"/>'s in the space.
    /// </summary>
    Task ClearAsync();
}

public interface ISpaceAgent<T, TTuple, TTemplate>
    where T : unmanaged
    where TTuple : ISpaceTuple<T>
    where TTemplate : ISpaceTemplate<T>
{
    Guid Subscribe(ISpaceObserver<TTuple> observer);
    void Unsubscribe(Guid observerId);

    Task WriteAsync(TTuple tuple);
    ValueTask EvaluateAsync(Func<Task<TTuple>> evaluation);
    
    ValueTask<TTuple> PeekAsync(TTemplate template);
    ValueTask PeekAsync(TTemplate template, Func<TTuple, Task> callback);
    
    ValueTask<TTuple> PopAsync(TTemplate template);
    ValueTask PopAsync(TTemplate template, Func<TTuple, Task> callback);

    ValueTask<IEnumerable<TTuple>> ScanAsync(TTemplate template);
    IAsyncEnumerable<TTuple> ConsumeAsync();
    ValueTask<int> CountAsync();
    Task ClearAsync();
}
