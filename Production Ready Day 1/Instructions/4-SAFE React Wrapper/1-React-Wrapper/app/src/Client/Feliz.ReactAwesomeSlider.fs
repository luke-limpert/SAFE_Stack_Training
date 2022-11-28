module Feliz.ReactAwesomeSlider

open Feliz
open Fable.Core.JsInterop

importAll "react-awesome-slider/dist/styles.css"
importAll "react-awesome-slider/dist/custom-animations/cube-animation.css"
importAll "react-awesome-slider/dist/custom-animations/fall-animation.css"
importAll "react-awesome-slider/dist/custom-animations/fold-out-animation.css"
importAll "react-awesome-slider/dist/custom-animations/open-animation.css"
importAll "react-awesome-slider/dist/custom-animations/scale-out-animation.css"

let reactAwesomeSlider: obj = importDefault "react-awesome-slider"

type AwesomeSlider =


    static member inline create props =
        Interop.reactApi.createElement(reactAwesomeSlider, createObj !!props)