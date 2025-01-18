namespace fsharp_file_api.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open fsharp_file_api
open System.IO
open Microsoft.AspNetCore.Http

[<Controller>]
[<Route("file-api")>]
type FileManagementController (logger : ILogger<FileManagementController>) =
    inherit ControllerBase()

    [<HttpGet("ls")>]
    member __.Ls([<FromQuery>] dir: string) : ObjectResult =
        let fullDir = $"{Util.root}{dir}"
        if Directory.Exists(fullDir) then
            let files = Directory.GetFileSystemEntries(fullDir) |> Array.map (fun path -> {
                name = Path.GetFileName(path)
                isDir = Directory.Exists(path)
            })
            upcast __.Ok(files)
        else
            upcast __.NotFound({| msg = $"Directory {fullDir} not found" |})

    [<HttpPost("newDir")>]
    member __.NewDir(dir: string, name: string) : ObjectResult =
        let dirPath = $"{Util.root}{dir}{name}"
        if not (Directory.Exists(dirPath)) then
            Directory.CreateDirectory(dirPath) |> ignore
            upcast __.Ok({| msg = "Directory created" |})
        else
            upcast __.BadRequest({| msg = "Directory already exists" |})

    [<HttpPost("rename")>]
    member __.Rename(dir: string, oldName: string, newName: string) : ObjectResult =
        let oldPath = $"{Util.root}{dir}{oldName}"
        let newPath = $"{Util.root}{dir}{newName}"
        if File.Exists(oldPath) || Directory.Exists(oldPath) then
            File.Move(oldPath, newPath)
            upcast __.Ok({| msg = "Renamed successfully" |})
        else
            upcast __.NotFound({| msg = "File or directory not found" |})

    [<HttpPost("move")>]
    member __.Move(srcDir: string, dstDir: string, files: string) : ObjectResult =
        let mutable result: ObjectResult = __.Ok({| msg = "Moved successfully" |})

        for file in files.Split(",") do
            let srcPath = $"{Util.root}{srcDir}{file}"
            let dstPath = $"{Util.root}{dstDir}{file}"
            if File.Exists(srcPath) || Directory.Exists(srcPath) then
                File.Move(srcPath, dstPath)
            else
                result <- __.NotFound({| msg = $"File or directory '{file}' not found" |})
        result

    [<HttpPost("delete")>]
    member __.Delete(dir: string, files: string) : ObjectResult =
        let mutable result: ObjectResult = __.Ok({| msg = "Deleted successfully" |})

        for file in files.Split(",") do
            let path = $"{Util.root}{dir}{file}"
            if Directory.Exists(path) then
                Directory.Delete(path, true)
            elif File.Exists(path) then
                File.Delete(path)
            else
                result <- __.NotFound({| msg = $"File or directory '{file}' not found" |})
        result

    [<HttpPost("upload")>]
    member __.Upload(dir: string, file: IFormFile) : Async<ObjectResult> =
        async {
            let destPath = $"{Util.root}{dir}{file.FileName}"
            use stream = new FileStream(destPath, FileMode.Create)
            do! file.CopyToAsync(stream) |> Async.AwaitTask
            return __.Ok({| msg = "File uploaded successfully" |})
        }
