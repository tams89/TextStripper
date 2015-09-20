namespace a

module TextStripper = 
    open System.Net
    open System.Text.RegularExpressions
    open HtmlAgilityPack
    open System.IO
    open System

    // Get all text from within an element by id
    let getText url = 
        let wb = new WebClient()
        let html = wb.OpenRead(url : string)
        let doc = new HtmlDocument()
        doc.Load(html)
        doc.GetElementbyId("facts-text").InnerText

    // Example execution
    let savetextToFile =
        let links = new System.Collections.Generic.List<string>()
        let path sp = Environment.GetFolderPath(sp)
        let linksFile = path Environment.SpecialFolder.Desktop + "\\Links.txt"
        if File.Exists(linksFile) then
            let readLinks  = File.ReadAllLines(linksFile);
            if readLinks.Length > 0 then
                for l in readLinks do 
                    links.Add(l)

        let text = [ for link in links -> getText link ]
        let path = path Environment.SpecialFolder.Desktop + "\\ExtractedText.txt"

        for t in text do
            File.AppendAllText(path, t)
            File.AppendAllText(path, Environment.NewLine)
            File.AppendAllText(path, Environment.NewLine)

        printfn "Links extracted and saved"