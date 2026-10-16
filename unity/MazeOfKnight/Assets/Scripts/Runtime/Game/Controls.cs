using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MazeOfKnight.Game
{
    // Los botones del juego, con teclado y con mando, que se pueden cambiar en Opciones (Mando y Teclado, como en Hollow
    // Knight; se guardan en PlayerPrefs). Moverse y apuntar van siempre con las flechas, el stick izquierdo o la cruceta;
    // con RemakeRules.HollowSlashes se salta con Saltar (Z, A) y ↑ apunta; sin ella ↑ salta, como en el original. Pausa
    // es Esc o Start. Cada botón es una InputAction con su tecla y su
    // botón del mando; PlayerController y GameBootstrap las leen de aquí.
    public static class Controls
    {
        public sealed class Def
        {
            public string Id, Name, Key, Pad;     // Key/Pad: ruta por defecto (null: no se usa en ese dispositivo)
        }

        public static readonly Def[] All =
        {
            new Def { Id = "saltar", Name = "Saltar", Key = "<Keyboard>/z", Pad = "<Gamepad>/buttonSouth" },
            new Def { Id = "espada", Name = "Atacar", Key = "<Keyboard>/space", Pad = "<Gamepad>/buttonWest" },
            new Def { Id = "concentrar", Name = "Concentrarse", Key = "<Keyboard>/f", Pad = "<Gamepad>/buttonEast" },
            new Def { Id = "arma", Name = "Arma", Key = "<Keyboard>/m", Pad = "<Gamepad>/buttonNorth" },
            new Def { Id = "biblia", Name = "Biblia", Key = "<Keyboard>/leftCtrl", Pad = "<Gamepad>/leftShoulder" },
            new Def { Id = "palabras", Name = "Escribir palabras", Key = null, Pad = "<Gamepad>/rightShoulder" },
            new Def { Id = "inventario", Name = "Inventario", Key = "<Keyboard>/i", Pad = "<Gamepad>/select" },
        };

        static readonly Dictionary<string, InputAction> actions = new Dictionary<string, InputAction>();
        public static event Action Changed;

        static string Pref(string id, bool pad) => (pad ? "mando_" : "tecla_") + id;

        public static Def Get(string id) => Array.Find(All, d => d.Id == id);

        public static string PathOf(string id, bool pad)
        {
            var d = Get(id);
            string def = pad ? d?.Pad : d?.Key;
            return def == null ? null : PlayerPrefs.GetString(Pref(id, pad), def);
        }

        // La acción de ese botón (habilitada), con su tecla y su botón del mando de ahora.
        public static InputAction Action(string id)
        {
            if (actions.TryGetValue(id, out var a)) return a;
            a = new InputAction(id, InputActionType.Button);
            foreach (bool pad in new[] { false, true })
                if (PathOf(id, pad) is string p) a.AddBinding(p);
            a.Enable();
            actions[id] = a;
            return a;
        }

        public static bool Pressed(string id) => Action(id).WasPressedThisFrame();

        public static void Rebind(string id, bool pad, string path)
        {
            PlayerPrefs.SetString(Pref(id, pad), path);
            Rebuild();
        }

        public static void Reset(bool pad)
        {
            foreach (var d in All) PlayerPrefs.DeleteKey(Pref(d.Id, pad));
            Rebuild();
        }

        static void Rebuild()
        {
            foreach (var a in actions.Values) { a.Disable(); a.Dispose(); }
            actions.Clear();
            Changed?.Invoke();
        }

        // Lo que se pulsa ahora para cambiar un botón: una tecla (Esc cancela: devuelve "") o un botón del mando (sin la
        // cruceta, los sticks ni Start, que son fijos). Null mientras no se pulse nada.
        public static string Listen(bool pad)
        {
            if (!pad)
            {
                var kb = Keyboard.current;
                if (kb == null) return null;
                if (kb.escapeKey.wasPressedThisFrame) return "";
                foreach (var k in kb.allKeys)
                    if (k.wasPressedThisFrame && k.keyCode != Key.UpArrow && k.keyCode != Key.DownArrow && k.keyCode != Key.LeftArrow && k.keyCode != Key.RightArrow)
                        return "<Keyboard>/" + k.name;
                return null;
            }
            var gp = Gamepad.current;
            if (gp == null) return null;
            if (gp.startButton.wasPressedThisFrame) return "";
            foreach (var b in new ButtonControl[] { gp.buttonSouth, gp.buttonEast, gp.buttonWest, gp.buttonNorth, gp.leftShoulder, gp.rightShoulder,
                                                    gp.leftTrigger, gp.rightTrigger, gp.selectButton, gp.leftStickButton, gp.rightStickButton })
                if (b.wasPressedThisFrame) return "<Gamepad>/" + b.name;
            return null;
        }

        // Nombre corto de un botón para enseñarlo (con los nombres de Xbox, como los dibujos del mando).
        public static string Label(string path)
        {
            if (string.IsNullOrEmpty(path)) return "—";
            string c = path.Substring(path.IndexOf('/') + 1);
            if (path.StartsWith("<Gamepad>"))
                return c switch
                {
                    "buttonSouth" => "A", "buttonEast" => "B", "buttonWest" => "X", "buttonNorth" => "Y",
                    "leftShoulder" => "LB", "rightShoulder" => "RB", "leftTrigger" => "LT", "rightTrigger" => "RT",
                    "select" => "Select", "start" => "Start", "leftStickPress" => "L3", "rightStickPress" => "R3",
                    _ => c,
                };
            return c switch
            {
                "space" => "Espacio", "leftCtrl" => "Ctrl izq.", "rightCtrl" => "Ctrl dcho.", "leftShift" => "Mayús izq.",
                "rightShift" => "Mayús dcha.", "leftAlt" => "Alt izq.", "rightAlt" => "Alt dcho.", "enter" => "Intro",
                "tab" => "Tab", "backspace" => "Retroceso",
                _ => c.Length == 1 ? c.ToUpperInvariant() : char.ToUpperInvariant(c[0]) + c.Substring(1),
            };
        }
    }
}
