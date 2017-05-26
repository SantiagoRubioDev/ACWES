namespace TestCode

module StudentsAnswers =

    type Answer = 
        { 
            testQ1: unit -> int -> int 
            testQ2: unit -> float -> float -> float
            testQ3: unit -> (int -> int) -> int -> int 
        }

    ////////-----------------Write your Functions Here---------------//////////////
    let answer1 x= x
    
    
    
    
    
    ////////-----------------Add your function to the related question---------------//////////////
    let studentAnswer : Answer = 
        {   
            //Q1. Write a function int->int that returns the absolute value of its integer parameter.
                testQ1 = fun () -> answer1 
            //Q2. Write a function f: float->float->float such that f a b=a∗a+b∗b−−−−−−−−−−√f a b=a∗a+b∗b
                testQ2 = fun () -> failwith "Answer not implemented"
            //Q3. Write a function threeTimes f (x:int) that computes f3(x)f3(x).
                testQ3 = fun () -> failwith "Answer not implemented"
        }
    

