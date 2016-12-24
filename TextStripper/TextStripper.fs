namespace a

module TextStripper = 
    open HtmlAgilityPack
    open System
    open System.IO
    open System.Net
    open System.Text.RegularExpressions
    open System.Xml

    // Get all text from within an element by id
    let getText domain = 
        use wb = new WebClient()
        wb.DownloadString("http://www.siteworthtraffic.com/update-report/" + domain) |> ignore
        let html = wb.OpenRead("http://www.siteworthtraffic.com/report/" + domain : string)
        let doc = new HtmlDocument()
        doc.Load(html)
        let paragraph = (doc.DocumentNode.Descendants("p") |> Seq.skip 1 |> Seq.head).InnerText
        let tables = [ for table in doc.DocumentNode.Descendants("table") -> table.InnerText ]
        (tables  |> Seq.append [ paragraph ], domain)
    
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

            // Create the document
            let xmlDoc = new XmlDocument()
            let xmlPath = Environment.GetFolderPath Environment.SpecialFolder.Desktop + "\\Extracted.xml"

            let dec = xmlDoc.CreateXmlDeclaration("1.0", null, null);
            xmlDoc.AppendChild dec |> ignore

            let parentNode = xmlDoc.CreateElement "root"
            xmlDoc.AppendChild parentNode |> ignore

            let writeTextToFile tuple = 
                let link = tuple |> snd
                let t = tuple |> fst
                let domainNode = xmlDoc.CreateElement "domain"

                let linkNode = xmlDoc.CreateElement "link"
                linkNode.InnerText <- link

                domainNode.AppendChild linkNode |> ignore
                parentNode.AppendChild domainNode |> ignore

                for block in t do
                    let paragraphNode = xmlDoc.CreateElement "paragraph"
                    paragraphNode.InnerText <- block
                    domainNode.AppendChild (paragraphNode) |> ignore

                xmlDoc.Save(xmlPath)

            for link in links do
                let textBlocks = getText link
                writeTextToFile textBlocks

            printfn "Links extracted and saved"

        with :? System.Exception as ex -> printfn "%A" ex.InnerException