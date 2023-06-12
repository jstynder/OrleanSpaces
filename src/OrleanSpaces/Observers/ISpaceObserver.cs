﻿using OrleanSpaces.Tuples;

namespace OrleanSpaces.Observers;

public interface ISpaceObserver
{
    /// <summary>
    /// Occurs whenever a <see cref="SpaceTuple"/> is written in the space.
    /// </summary>
    /// <param name="tuple">The written tuple.</param>
    /// <param name="cancellationToken">A token used to propagate cancellations.</param>
    Task OnExpansionAsync(SpaceTuple tuple, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs whenever a <see cref="SpaceTuple"/> is removed from the space.
    /// </summary>
    /// <param name="template">The template that was used to remove the tuple.</param>
    ///<param name="cancellationToken">A token used to propagate cancellations.</param>
    Task OnContractionAsync(SpaceTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs whenever the space contains zero <see cref="SpaceTuple"/>'s.
    /// </summary>
    /// <param name="cancellationToken">A token used to propagate cancellations.</param>
    Task OnFlatteningAsync(CancellationToken cancellationToken = default);
}

public interface ISpaceObserver<T, TTuple, TTemplate>
    where T : unmanaged
    where TTuple : ISpaceTuple<T>
    where TTemplate : ISpaceTemplate<T>
{
    /// <summary>
    /// Occurs whenever a <see cref="TTuple"/> is written in the space.
    /// </summary>
    /// <param name="tuple">The written tuple.</param>
    /// <param name="cancellationToken">A token used to propagate cancellations.</param>
    Task OnExpansionAsync(TTuple tuple, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs whenever a <see cref="TTuple"/> is removed from the space.
    /// </summary>
    /// <param name="template">The template that was used to remove the tuple.</param>
    ///<param name="cancellationToken">A token used to propagate cancellations.</param>
    Task OnContractionAsync(TTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs whenever the space contains zero <see cref="TTuple"/>'s.
    /// </summary>
    /// <param name="cancellationToken">A token used to propagate cancellations.</param>
    Task OnFlatteningAsync(CancellationToken cancellationToken = default);
}
