namespace a

module TextStripper = 
    open HtmlAgilityPack
    open System
    open System.IO
    open System.Net
    open System.Text.RegularExpressions
    open FSharp.ExcelProvider
    
    // Get all text from within an element by id
    let getText domain = 
        let wb = new WebClient()
        wb.OpenRead("http://www.siteworthtraffic.com/update-report/" + domain) |> ignore
        let html = wb.OpenRead("http://www.siteworthtraffic.com/report/" + domain : string)
        let doc = new HtmlDocument()
        doc.Load(html)
        let paragraph = (doc.DocumentNode.Descendants("p") |> Seq.skip 1 |> Seq.head).InnerText
        let tables = [ for table in doc.DocumentNode.Descendants("table") -> table.InnerText ]
        tables  |> Seq.append [ paragraph ] 
    
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
                let textBlocks = getText link
                for text in textBlocks do
                    writeTextToFile text
            printfn "Links extracted and saved"

        with :? System.Exception -> printfn "Unhandled application error. Please prod Fatman."
