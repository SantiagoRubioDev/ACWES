# ACWES
ACWES (Automated CourseWork Evaluation Service) is a web application that helps academics mark their students programming coursework semi-automatically. This project is my Masters degree final year project at Imperial College London

# Intro
The main point of this project is to make a functional website using F# as the primary programming language. The reason I use F# is its very simple and beautiful syntax. The project uses Fable (a F# to JavaScript compiler) and Suave (a simple web development F# library) as the two main technologies for development.
This project was built on top of one of many Fable sample projects, the fable-suave-scaffold project (https://github.com/fable-compiler/fable-suave-scaffold). 
The Fable and Suave developers and gitter chat memebers helped me at the begining of the project when I was still getting used to their technologies, so Thank you to all of them.

# Running the app on your system
You can try the project on your system simply by downloading the repo and running the command
```
> build.cmd run // on windows
$ ./build.sh run // on unix
```
This command will automatically download all packages and dependencies required to run the application. It will also open the web page on your browser.

This command calls build.fsx with target "Run". It will start in parallel:

- dotnet watch run in src/Server (note: Suave is launched on port 8085)
- dotnet fable npm-run start in src/Client (note: the Webpack development server will server files on http://localhost:8080)

You can now edit files in src/Server or src/Client and recompile. Browser refresh will be triggered automatically.

For development convinency src/Client/ACWESDev.fsproj links files from both src/Client/Client.fsproj and src/Server/Server.fsproj.


