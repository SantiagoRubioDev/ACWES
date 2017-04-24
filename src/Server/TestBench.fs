module ServerCode.TestBench

open Newtonsoft.Json
open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.ServerErrors
open Suave.Logging
open Suave.Logging.Message
open ServerCode.Domain

let logger = Log.create "FableSample"