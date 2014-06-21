using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.CSharp;
using SharpKit.JavaScript.Ast;
using ICSharpCode.NRefactory.TypeSystem;

namespace SharpKit.Compiler
{
    /// <summary>
    /// Compiler events by order:
    ///     ParseCs
    ///     ApplyExternalMetadata
    ///     ConvertCsToJs
    ///     MergeJsFiles
    ///     InjectJsCode
    ///     OptimizeJsFiles
    ///     SaveJsFiles
    ///     EmbedResources
    ///     SaveNewManifest
    /// </summary>
    public interface ICompiler
    {
        #region Events

        event Action BeforeParseCs;
        event Action BeforeApplyExternalMetadata;
        event Action BeforeConvertCsToJs;
        event Action BeforeMergeJsFiles;
        event Action BeforeInjectJsCode;
        event Action BeforeOptimizeJsFiles;
        event Action BeforeSaveJsFiles;
        event Action BeforeEmbedResources;
        event Action BeforeSaveNewManifest;
        event Action BeforeExit;

        event Action AfterParseCs;
        event Action AfterApplyExternalMetadata;
        event Action AfterConvertCsToJs;
        event Action AfterMergeJsFiles;
        event Action AfterInjectJsCode;
        event Action AfterOptimizeJsFiles;
        event Action AfterSaveJsFiles;
        event Action AfterEmbedResources;
        event Action AfterSaveNewManifest;


        event Action<IEntity> BeforeConvertCsToJsEntity;
        event Action<IEntity, JsNode> AfterConvertCsToJsEntity;

        event Action<AstNode> BeforeConvertCsToJsAstNode;
        event Action<AstNode, JsNode> AfterConvertCsToJsAstNode;

        event Action<ResolveResult> BeforeConvertCsToJsResolveResult;
        event Action<ResolveResult, JsNode> AfterConvertCsToJsResolveResult;

        #endregion

        #region Properties

        ICompilation CsCompilation { get; }
        List<SkJsFile> SkJsFiles { get; }
        ICustomAttributeProvider CustomAttributeProvider { get; }
        #endregion

    }
}
