namespace a

module TextStripper = 
    open HtmlAgilityPack
    open System
    open System.IO
    open System.Net
    open System.Text.RegularExpressions
    open Microsoft.Office.Interop
    
    // Get all text from within an element by id
    let getText domain = 
        use wb = new WebClient()
        wb.DownloadString("http://www.siteworthtraffic.com/update-report/" + domain) |> ignore
        let html = wb.OpenRead("http://www.siteworthtraffic.com/report/" + domain : string)
        let doc = new HtmlDocument()
        doc.Load(html)
        let paragraph = (doc.DocumentNode.Descendants("p") |> Seq.skip 1 |> Seq.head).InnerText
        let tables = [ for table in doc.DocumentNode.Descendants("table") -> table.InnerText ]
        tables  |> Seq.append [ paragraph ]  |> Seq.append [ domain ] 
    
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

            let xlApp = new Excel.ApplicationClass()
            let excelPath = Environment.GetFolderPath Environment.SpecialFolder.Desktop + "\\Extracted.xlsx"
            let xlWorkBook = xlApp.Workbooks.Open(excelPath)
            xlApp.Visible <- false
            let xlWorkSheet = xlWorkBook.Worksheets.["Sheet1"] :?> Excel.Worksheet

            let writeTextToFile (t : string []) = 
                let rec row index (text : string []) = 
                    let emptyRow = (xlWorkSheet.Cells.Rows.[index] :?> Excel.Range)
                    let emptyRow = (xlWorkSheet.Cells.Rows.[2] :?> Excel.Range)
                    if (emptyRow.Cells.Value2 |> string) = "" then 
                        emptyRow.Value2 <- text
                    else
                        row (index + 1) text
                row 1 t

            for link in links do
                let textBlocks = getText link
                writeTextToFile (textBlocks |> Seq.toArray)

            xlWorkSheet.SaveAs(excelPath)
            xlApp.Workbooks.Close()
            printfn "Links extracted and saved"

        with :? System.Exception as ex -> printfn "%A" ex.InnerException