#r "System.Xml.Linq.dll"
#r "../../../bin/lib/net45/FSharp.Data.dll"
open System.Xml.Linq
open FSharp.Data


(*JSON TO XML*)

/// Creates a JSON representation of a XML element
let rec fromXml (xml:XElement) =

  // Create a collection of key/value pairs for all attributes      
  let attrs = 
    [ for attr in xml.Attributes() ->
        (attr.Name.LocalName, JsonValue.String attr.Value) ]

  // Function that turns a collection of XElement values
  // into an array of JsonValue (using fromXml recursively)
  let createArray xelems =
    [| for xelem in xelems -> fromXml xelem |]
    |> JsonValue.Array

  // Group child elements by their name and then turn all single-
  // element groups into a record (recursively) and all multi-
  // element groups into a JSON array using createArray
  let children =
    xml.Elements() 
    |> Seq.groupBy (fun x -> x.Name.LocalName)
    |> Seq.map (fun (key, childs) ->
        match Seq.toList childs with
        | [child] -> key, fromXml child
        | children -> key + "s", createArray children )
        
  // Concatenate elements produced for child elements & attributes
  Array.append (Array.ofList attrs) (Array.ofSeq children)
  |> JsonValue.Record


(* XML to JSON *)
  /// Creates an XML representation of a JSON value (works 
/// only when the top-level value is an object or an array)
let toXml(x:JsonValue) =
  // Helper functions for constructing XML 
  // attributes and XML elements
  let attr name value = 
    XAttribute(XName.Get name, value) :> XObject
  let elem name (value:obj) = 
    XElement(XName.Get name, value) :> XObject

  // Inner recursive function that implements the conversion
  let rec toXml = function
    // Primitive values are returned as objects
    | JsonValue.Null -> null
    | JsonValue.Boolean b -> b :> obj
    | JsonValue.Number number -> number :> obj
    | JsonValue.Float number -> number :> obj
    | JsonValue.String s -> s :> obj

    // JSON object becomes a collection of XML
    // attributes (for primitives) or child elements
    | JsonValue.Record properties -> 
      properties 
      |> Array.map (fun (key, value) ->
          match value with
          | JsonValue.String s -> attr key s
          | JsonValue.Boolean b -> attr key b
          | JsonValue.Number n -> attr key n
          | JsonValue.Float n -> attr key n
          | _ -> elem key (toXml value)) :> obj

    // JSON array is turned into a 
    // sequence of <item> elements
    | JsonValue.Array elements -> 
        elements |> Array.map (fun item -> 
          elem "item" (toXml item)) :> obj

  // Perform the conversion and cast the result to sequence
  // of objects (may fail for unexpected inputs!)
  (toXml x) :?> XObject seq