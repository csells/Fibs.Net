import Html exposing (..)
import Html.App as App
import Html.Attributes exposing (..)
import Html.Events exposing (..)
import WebSocket

main = App.program { init = init, view = view, update = update, subscriptions = subs }
fibsProxy = "ws://localhost:62953/fibs"

-- MODEL
type alias Model = { input : String, messages : List String }

init : (Model, Cmd Msg)
init = (Model "" [], Cmd.none)

-- VIEW
view : Model -> Html Msg
view model = div []
    [ input [onInput Input, value model.input] []
    , button [onClick Send] [text "Send"]
    , div [] (List.map viewMessage (List.reverse model.messages))
    ]

viewMessage : String -> Html msg
viewMessage msg = div [] [ text msg ]

-- UPDATE
type Msg = Input String | Send | NewMessage String

update : Msg -> Model -> (Model, Cmd Msg)
update msg {input, messages} = case msg of
  Input newInput -> (Model newInput messages, Cmd.none)
  Send -> (Model "" messages, WebSocket.send fibsProxy input)
  NewMessage str -> (Model input (str :: messages), Cmd.none)

-- SUBSCRIPTIONS
subs : Model -> Sub Msg
subs model = WebSocket.listen fibsProxy NewMessage
