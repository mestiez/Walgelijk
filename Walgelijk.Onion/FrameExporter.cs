using Newtonsoft.Json;
using System.Diagnostics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion;

public static class FrameExporter
{
    [Command(Alias ="OnionSnap")]
    public static void Export(string path = "snapshot.html")
    {
        using var doc = new StreamWriter(path);
        int nodesWritten = 0;
        List<object> list = new();
        renderChildren(Onion.Tree.Root);

        doc.WriteLine(@"<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Onion frame snapshot</title>
    <style>
        html,
        body {
            display: grid;
            grid-template-columns: 1fr 1fr;
            place-items: center;
            margin: 0;
            padding: 0;
            background: #b5bdc9;
            width: 100vw;
            min-height: 100vh;
        }

        .window {
            margin: 1em;
            position: relative;
            background: black;
            outline: 2px solid mediumpurple;
            border-top: 30px solid mediumpurple;
        }

        .info {}

        .property {
            display: grid;
            font-family: monospace;
            grid-template-columns: min-content 1fr;
            gap: 1em;
            background: #8080802b;
            outline: 1px dotted;
        }

        .property *:nth-child(1) {
            font-weight: bold;
            text-align: right;
        }

        .property *:nth-child(2) {
            font-weight: normal;
        }

        details {
            cursor: pointer;
        }

        details:not([open]) {
            opacity: 0.5;
        }

        details[open] > summary{
            opacity: 0.1;
        }

        .control {
            position: absolute;
            background: rgba(10, 53, 51, 0.8);
            outline: 1px dashed mediumturquoise;
            color: rgba(72, 209, 205, 0.5);
            font-family: monospace;
            overflow: hidden;
            padding: 5px;
            box-sizing: border-box;
        }

        .control:hover {
            background: rgba(72, 209, 205, 0.3);
            outline: 1px solid mediumturquoise;
        }

        .control[selected] {
            background: rgba(147, 112, 216, 0.7);
            color: rgb(203, 178, 255);
            outline-color: white;
        }
    </style>
</head>

<body>
    <div class=""window""></div>
    <div class=""info""></div>
    <script>");
        doc.WriteLine($"const windowElem = document.querySelector('.window');");
        doc.WriteLine($"windowElem.style.width = '{Game.Main.Window.Width}px'");
        doc.WriteLine($"windowElem.style.height = '{Game.Main.Window.Height}px'");
        doc.WriteLine("const controls = {0};", JsonConvert.SerializeObject(list, Formatting.Indented));
        doc.WriteLine(@"  </script>
    <script>
        let hover = null;
        let selected = null;

        const w = document.getElementsByClassName('window')[0];
        controls.sort((a, b) => {
            return a.order - b.order;
        });
        for (const c of controls) {
            const ee = document.createElement('div');
            ee.classList.add('control');
            ee.setAttribute('id', c.id);
            ee.textContent = c.behaviour;
            ee.style.width = `${c.rect.MaxX - c.rect.MinX}px`;
            ee.style.height = `${c.rect.MaxY - c.rect.MinY}px`;
            ee.style.left = `${c.rect.MinX}px`;
            ee.style.top = `${c.rect.MinY}px`;
            ee.addEventListener('mouseenter', () => { setHover(c); });
            ee.addEventListener('mouseleave', () => { if (hover == c) setHover(null); });
            ee.addEventListener('click', () => { setSelected(c); });
            w.append(ee);
        }

        function setHover(v) {
            if (v && v._noHover)
                return;
            hover = v;
        }

        function setSelected(v) {
            if (v)
                console.log(v);
            selected = v;
            for (const iterator of document.querySelectorAll('.control')) {
                if (selected && iterator.id == selected.id)
                    iterator.setAttribute('selected', null)
                else
                    iterator.removeAttribute('selected')
            }

            const infoElement = document.querySelector('.info');
            infoElement.innerHTML = '';
            const drawObject = (obj, element) => {
                for (const key in obj) {
                    if (Object.hasOwnProperty.call(obj, key)) {
                        const data = obj[key];
                        const e = document.createElement('div');
                        e.classList.add('property');

                        const label = document.createElement('span');
                        label.textContent = key;
                        e.append(label);

                        let dataElem;
                        if (data && (data instanceof Object || (typeof (data) == ""string"" && data.length > 32))) {
                            dataElem = document.createElement('details');
                            if (typeof (data) == ""string"") {
                                const summary = document.createElement('summary');
                                summary.innerText = data.substring(0, Math.min(32, data.length - 10)) + '...';
                                dataElem.innerText = data;
                                dataElem.append(summary);
                            } else {
                                var dataKeys = Object.keys(data);
                                var dataPreview = JSON.stringify(data);
                                const summary = document.createElement('summary');
                                dataElem.append(summary);
                                summary.innerText = data.name || (dataPreview.length < 64 ? dataPreview : `(${dataKeys.length})`);
                                drawObject(data, dataElem);
                            }
                        }
                        else {
                            dataElem = document.createElement('span')
                            dataElem.textContent = data == null ? 'null' : data;
                        }
                        e.append(dataElem);

                        // check if data is control ID
                        if (typeof (data) == ""string""){
                            const matches = controls.filter(c => c.id == data)
                            if (matches.length == 1)
                            {
                                var match = matches[0];
                                dataElem.style.textDecoration = 'underline'
                                dataElem.style.color = 'purple'
                                dataElem.style.cursor = 'pointer';
                                dataElem.addEventListener('click', () => {
                                    setSelected(match);
                                })
                            }
                        }

                        element.append(e);
                    }
                }
            };
            drawObject(v, infoElement);
        }
    </script>
</body>

</html>");

        void renderChildren(Node n)
        {
            if (!n.Alive)
                return;
            render(n);
            foreach (var child in n.GetChildren())
                renderChildren(child);
        }
        
        void render(Node node)
        {
            nodesWritten++;
            var instance = node.GetInstance();
            var r = instance.Rects.ComputedDrawBounds;

            list.Add(new
            {
                id = node.Identity,
                name = instance.Name,
                behaviour = node.Behaviour.GetType().Name,
                order = node.ComputedGlobalOrder,
                alive = node.Alive,
                rect = instance.Rects.ComputedDrawBounds.Intersect(instance.Rects.ComputedGlobal),
                rects = instance.Rects,
                children = node.Children.Select(a => a.ToString()),
                parent = node.Parent?.Identity.ToString() ?? null,
                alwaysOnTop = node.AlwaysOnTop,
                chronologicalPosition = node.ChronologicalPosition,
                constraints = node.SelfLayout?.Select(c => new { name = c.GetType().Name, data = c }) ?? null,
                layouts = node.ChildrenLayout?.Select(c => new { name = c.GetType().Name, data = c }) ?? null,
                theme = new
                {
                    foreground = instance.Theme.Foreground[instance.State].Color.ToHexCode(),
                    background = instance.Theme.Background[instance.State].Color.ToHexCode(),
                    accent = instance.Theme.Accent[instance.State].ToHexCode(),
                    text = instance.Theme.Text[instance.State].ToHexCode(),
                    outlineWidth = instance.Theme.OutlineWidth[instance.State],
                    outlineColour = instance.Theme.OutlineColour[instance.State].ToHexCode(),
                    font = instance.Theme.Font.Name,
                    fontSize = instance.Theme.Font.Size,
                }
            });

           // doc.WriteLine(list);
        }

        doc.Dispose();
        Logger.Log($"Onion frame shapshot exported to \"{path}\" ({nodesWritten} controls)");
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}
