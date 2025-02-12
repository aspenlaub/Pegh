﻿using System;
using System.Threading.Tasks;
// ReSharper disable UnusedMemberInSuper.Global

namespace Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

public interface ISecretRepository {
    Task SetAsync<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new();
    Task<TResult> GetAsync<TResult>(ISecret<TResult> secret, IErrorsAndInfos errorsAndInfos) where TResult : class, ISecretResult<TResult>, new();
    Task<Func<TArgument, TResult>> CompileCsLambdaAsync<TArgument, TResult>(ICsLambda csLambda);
    string FileName(IGuid secret, bool sample);
}