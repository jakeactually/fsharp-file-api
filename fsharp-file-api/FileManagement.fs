namespace fsharp_file_api

open System
open System.IO

type File = {
    name: string
    isDir: bool
}

module Util =
    let root =
        Environment.GetCommandLineArgs()
        |> Array.skip(1)
        |> Array.tryHead
        |> Option.defaultValue "C:/Users/USER/Desktop/files"
