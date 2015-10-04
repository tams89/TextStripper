namespace a

module TextStripper = 
    open HtmlAgilityPack
    open System
    open System.IO
    open System.Net
    open System.Text.RegularExpressions
    
    // Get all text from within an element by id
    let getText url = 
        try 
            let wb = new WebClient()
            let html = wb.OpenRead(url : string)
            let doc = new HtmlDocument()
            doc.Load(html)
            doc.GetElementbyId("facts-text").InnerText
        with
        | :? System.Net.WebException -> 
            printfn "Error getting text from url: %A" url
            ""
        | :? System.NullReferenceException -> 
            printfn "Error getting text from url: %A" url
            ""
    
    // Example execution
    let savetextToFile() = 
        try 
            let links = new System.Collections.Generic.List<string>()
            let path sp = Environment.GetFolderPath(sp)
            let linksFile = path Environment.SpecialFolder.Desktop + "\\Links.txt"
            if File.Exists(linksFile) then 
                let readLinks = File.ReadAllLines(linksFile) |> Array.filter(fun l -> not(String.IsNullOrEmpty l))
                if readLinks.Length > 0 then 
                    for l in readLinks do
                        links.Add(l)
            let path = path Environment.SpecialFolder.Desktop + "\\ExtractedText.txt"
            
            let writeTextToFile t = 
                if not (String.IsNullOrEmpty t) then
                    File.AppendAllText(path,t)
                    File.AppendAllText(path,Environment.NewLine)
                    File.AppendAllText(path,Environment.NewLine)
            
            for link in links do
                getText link |> writeTextToFile 
            
            printfn "Links extracted and saved"

        with :? System.Exception -> printfn "Unhandled application error. Please prod Fatman."
