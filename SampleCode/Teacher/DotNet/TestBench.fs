namespace TestCode

module TestBench = 
    
    /// the correct answer
    let expected = ModelAnswers.modelAnswer//Model.Model.modelAnswer

    /// the current answer to test
    let actual = StudentsAnswers.studentAnswer
    
    [<EntryPoint>]
    let main argv = 
        let positiveInts = [1;2;3]
        let negativeInts = [-1;-5]
        let positiveFloats = [0.1;7.3;123.69]
        let negativeFloats = [-0.25;-1.9;-4.8]
        let functionList = [(fun x -> x);(fun x -> x+x);(fun x -> x*x)]

        let uniformMark actualQ expectedQ inputList=
            let resultActual = List.map (actualQ()) inputList
            let resultExpected = List.map (expectedQ()) inputList
            let equals x y i = 
                let bol = (x = y) 
                if not bol then 
                    printfn "Failed: expected %A but got %A for input %A \n" y x i
                bol
            let result = List.map3 equals resultActual resultExpected inputList
            List.fold (fun acc x -> if x=true then acc+1 else acc) 0 result

        let precisionMark actualQ expectedQ input1List input2List=
            let resultActual = List.map2 (actualQ()) input1List input2List
            let resultExpected = List.map2 (expectedQ()) input1List input2List
            let almostEquals = fun x y -> (x < y + 0.00001) && (x > y - 0.00001)
            let result = List.map2 almostEquals resultActual resultExpected
            List.fold (fun acc x -> if x=true then acc+1 else acc) 0 result

        let binaryMark actualQ expectedQ input1List input2List=
            let resultActual = List.map2 (actualQ()) input1List input2List
            let resultExpected = List.map2 (expectedQ()) input1List input2List
            let result = List.map2 (=) resultActual resultExpected
            let tempMark = List.fold (fun acc x -> if x=true then acc+1 else acc) 0 result
            if tempMark = input1List.Length then tempMark else 0

        let markQ1 = uniformMark actual.testQ1 expected.testQ1 (positiveInts@negativeInts)
        let markQ2 = precisionMark actual.testQ2 expected.testQ2 (positiveFloats@negativeFloats) (positiveFloats@negativeFloats)
        let markQ3 = binaryMark actual.testQ3 expected.testQ3 functionList positiveInts

        let maxMark = 14
        let totalMark = (markQ1+markQ2+markQ3)*100/maxMark

        let totalFeedback = "Q1 "+markQ1.ToString()+"/5 \n"+
                            "Q2 "+markQ2.ToString()+"/6 \n"+
                            "Q3 "+markQ3.ToString()+"/3"

        printfn "Mark %A \n" totalMark
        printfn "Feedback %A \n" totalFeedback
        //printfn "Hi %A %A" (actual.testQ1() -3) (expected.testQ1() -3) 
        0
