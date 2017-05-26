namespace TestCode

module StudentsAnswers2 =

    type Answer = 
        { 
            testQ1: unit -> int -> int 
            testQ2: unit -> float -> float -> float
            testQ3: unit -> (int -> int) -> int -> int 
        }

    ////////-----------------Write your Functions Here---------------//////////////

    //Q1. Write a function int->int that returns the absolute value of its integer parameter.
    let absolute= abs
    //Q2. Write a function f: float->float->float such that f a b=a∗a+b∗b−−−−−−−−−−√f a b=a∗a+b∗b
    let hypotenous (a:float) (b:float)  = sqrt ( (a*a) + (b*b) )
    //Q3. Write a function threeTimes f (x:int) that computes f3(x)f3(x).
    let threeTimes f x  = x |> f |> f |> f

    ////////-----------------Add your function to the related question---------------//////////////
    let studentAnswer : Answer = 
        {   
            //Q1. Write a function int->int that returns the absolute value of its integer parameter.
                testQ1 = fun () -> absolute 
            //Q2. Write a function f: float->float->float such that f a b=a∗a+b∗b−−−−−−−−−−√f a b=a∗a+b∗b
                testQ2 = fun () -> hypotenous
            //Q3. Write a function threeTimes f (x:int) that computes f3(x)f3(x).
                testQ3 = fun () -> threeTimes
        }
    

