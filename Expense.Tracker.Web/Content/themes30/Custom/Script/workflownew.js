var Graph = function (obj) {
    this.currentGroup;
    go.licenseKey = "73fe40e6b11c28c702d95d76423d6cbc5cf07f21de824da3045112a4ec5c6f162699ed7101d6dec986fc1cfb1c7990de8dc76a7a915e153db462dadd44b287adb53577b21101178bf10a72c7cbfd2ca3f97f70f0c0e57fa7da2fdef1bcab91940ebda3d5489a07bf2a6a1637032ea94ae5abd869e901cd4f6d729ab8f8";
    this.diagram = go.GraphObject.make;
    var self = this;

    var currentFont = "lighter 16px Open Sans";
    var boldFont = "bold 18px Open Sans";
    this.graph = this.diagram(go.Diagram, obj.elementId,
    {
        initialContentAlignment: go.Spot.Center,
        initialAutoScale: go.Diagram.Uniform,
        allowDrop: true,
        allowCopy: false,
        mouseDrop: function () { if (this.currentGroup) delete this.currentGroup; },
        "animationManager.isEnabled": false,
        "undoManager.isEnabled": true,
        "toolManager.mouseWheelBehavior": go.ToolManager.WheelNone,
        "linkingTool.linkValidation": checkLoops,
        "relinkingTool.linkValidation": checkLoops,
        "commandHandler.archetypeGroupData": { isGroup: true }
    });
    this.graph.grid =
       this.diagram(go.Panel, "Grid",
           { gridCellSize: new go.Size(10, 10) },
           this.diagram(go.Shape, "LineH", { strokeDashArray: [0.5, 9.5], strokeWidth: 0.5 })
       );
    var myOverview = this.diagram(go.Overview, "myOverviewDiv", { observed: self.graph });
    myOverview.box.elt(0).stroke = this.diagram(go.Brush, { color: "palegreen" });
    var buttonMouseOver = function (e, obj) {
        var shape = obj.findObject("ButtonBorder");
        shape.fill = "#555";
        shape.stroke = "#555";
        var xline = obj.findObject("ButtonIcon");
        xline.stroke = "white";
    };
    var buttonMouseOut = function (e, obj) {
        var shape = obj.findObject("ButtonBorder");
        shape.fill = "gray";
        shape.stroke = "gray";
        var xline = obj.findObject("ButtonIcon");
        xline.stroke = "white";
    };
    go.GraphObject.defineBuilder("SubGraphExpanderButton", function (args) {
        var button = /** @type {Panel} */ (
          self.diagram("Button",
              {
                  "_subGraphExpandedFigure": "MinusLine",
                  "_subGraphCollapsedFigure": "PlusLine",
                  "_buttonFillNormal": "#555",
                  "_buttonStrokeNormal": "#555",
                  "_buttonFillOver": "#555",
                  "_buttonStrokeOver": "#555",
                  mouseEnter: buttonMouseOver,
                  mouseLeave: buttonMouseOut
              },
              self.diagram(go.Shape,  // the icon
                {
                    name: "ButtonIcon",
                    figure: "MinusLine",  // default value for isSubGraphExpanded is true
                    desiredSize: new go.Size(6, 6)
                },
                // bind the Shape.figure to the Group.isSubGraphExpanded value using this converter:
                new go.Binding("figure", "isSubGraphExpanded",
                               function (exp, shape) {
                                   var button = shape.panel;
                                   return exp ? button["_subGraphExpandedFigure"] : button["_subGraphCollapsedFigure"];
                               })
                    .ofObject()))
        );
        // subgraph expand/collapse behavior
        button.click = function (e, button) {
            var group = button.part;
            if (group instanceof go.Adornment) group = group.adornedPart;
            if (!(group instanceof go.Group)) return;
            var diagram = group.diagram;
            if (diagram === null) return;
            var cmd = diagram.commandHandler;
            if (group.isSubGraphExpanded) {
                if (!cmd.canCollapseSubGraph(group)) return;
            } else {
                if (!cmd.canExpandSubGraph(group)) return;
            }
            e.handled = true;
            if (group.isSubGraphExpanded) {
                cmd.collapseSubGraph(group);
            } else {
                cmd.expandSubGraph(group);
            }
        };
        var border = button.findObject("ButtonBorder");
        if (border instanceof go.Shape) {
            border.stroke = "gray";
            border.fill = "transparent";
        }
        return button;
    });
    go.GraphObject.defineBuilder("CloseButton", function (args) {
        var eltname = (go.GraphObject.takeBuilderArgument(args, "COLLAPSIBLE"));

        var line = go.GraphObject.make(go.Shape, "XLine", { width: 5, height: 5, stroke: "#ff4444", strokeWidth: 2 })
        var button = go.GraphObject.make("Button",
            {
                alignment: go.Spot.TopRight,
                "_buttonFillNormal": "#555",
                "_buttonStrokeNormal": "#555",
                "_buttonFillOver": "#555",
                "_buttonStrokeOver": "#555"
            }, line);

        var border = button.findObject("ButtonBorder");
        if (border instanceof go.Shape) {
            border.stroke = null;
            border.fill = "transparent";
        }
        return button;
    });

    var model = this.graph.model;
    model.linkFromPortIdProperty = "fromPort";
    model.linkToPortIdProperty = "toPort";
    function showPorts(node, show) {
        var diagram = node.diagram;
        if (!diagram || diagram.isReadOnly || !diagram.allowLink) return;
        node.ports.each(function (port) {
            port.stroke = (show ? "black" : null);
        });
    };

    var isUnoccupied = function (r, node) {
        var diagram = node.diagram;

        // nested function used by Layer.findObjectsIn, below
        // only consider Parts, and ignore the given Node
        var navig = function (obj) {
            var part = obj.part;
            // i used part.data.from to check if a part is a link, only links have a from property
            if (part === node || part.data.from) return null;
            return part;
        }

        // only consider non-temporary Layers
        var lit = diagram.layers;
        while (lit.next()) {
            var lay = lit.value;
            if (lay.isTemporary) continue;
            if (lay.findObjectsIn(r, navig, null, true).count > 0) return false;
        }
        return true;
    };

    // a Part.dragComputation function that prevents a Part from being dragged to overlap another Part
    var avoidNodeOverlap = function (node, pt, gridpt) {
        var bnds = node.actualBounds;
        var loc = node.location;
        // see if the area at the proposed location is unoccupied
        var x = pt.x - (loc.x - bnds.x);
        var y = pt.y - (loc.y - bnds.y);
        //in my case new Rect didnt work
        var r = new go.Rect(x, y, bnds.width, bnds.height);
        if (isUnoccupied(r, node)) return pt;  // OK
        return loc;  // give up -- don't allow the node to be moved to the new location
    };
    var lightText = 'black';
    var adornmentTemplate = this.diagram(go.Adornment, "Spot",
            this.diagram(go.Panel, "Auto",
              // this Adornment has a rectangular blue Shape around the selected node
              this.diagram(go.Shape, { fill: null, stroke: "gray", strokeWidth: 2 }),
              this.diagram(go.Placeholder)
            ),
            // and this Adornment has a Button to the right of the selected node
             this.diagram("CloseButton", {})
          );
    var nodeStyle = [

       { cursor: "pointer", fromLinkableDuplicates: false, toLinkableDuplicates: false, fromLinkableSelfNode: false, toLinkableSelfNode: false },

          new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
          {
              locationSpot: go.Spot.Center,
              mouseEnter: function (e, obj) { showPorts(obj.part, true); },
              mouseLeave: function (e, obj) { showPorts(obj.part, false); }
          }
    ];


    function highlightGroup(e, grp, show) {
        if (!grp) return;
        e.handled = true;
        if (show) {
            // cannot depend on the grp.diagram.selection in the case of external drag-and-drops;
            // instead depend on the DraggingTool.draggedParts or .copiedParts
            var tool = grp.diagram.toolManager.draggingTool;
            var map = tool.draggedParts || tool.copiedParts;  // this is a Map
            // now we can check to see if the Group will accept membership of the dragged Parts
            if (grp.canAddMembers(map.toKeySet())) {
                grp.isHighlighted = true;
                return;
            }
        }
        grp.isHighlighted = false;
    }

    function finishDrop(e, grp) {

        currentGroup = grp;
    }


    function makePort(graph, name, spot, output, input) {
        return graph.diagram(go.Shape, "Circle",
            {
                fill: "transparent",
                stroke: null,  // this is changed to "white" in the showPorts function
                desiredSize: new go.Size(10, 10),
                alignment: spot, alignmentFocus: spot,  // align the port on the main Shape
                portId: name,  // declare this object to be a "port"
                fromSpot: spot, toSpot: spot,  // declare where links may connect at this port
                fromLinkable: output, toLinkable: input,  // declare whether the user may draw links to/from here
                cursor: "pointer",  // show a different cursor to indicate potential link point
                fromLinkableDuplicates: false, toLinkableDuplicates: false,
                fromLinkableSelfNode: true, toLinkableSelfNode: true
            });
    };


    this.graph.addDiagramListener("ExternalObjectsDropped", function (e) {
        obj.diagramUpdate(e);
    });
    this.graph.addDiagramListener("ChangedSelection", function (e) {
        obj.objectSelected(e);
    });
    this.graph.addDiagramListener("LinkDrawn", function (e) {
        obj.linkUpdate(e);
    });

    var myDig = this.graph;
    this.deleteSegmentwithConfirm = function (e, obj) {
        debugger;
        self.deleteSegment(e, obj, true);
    };
    this.deleteSegment = function (e, obj, showConfirm) {
        debugger;
        var node = obj.part;

        if (typeof showConfirm !== 'undefined' && showConfirm == true) {
            if (!confirm("Are you sure to delete " + node.data.text + " from the workflow?"))
                return;
        }

        myDig.remove(node);
        var group = node.containingGroup;

        // new code for deleting halt and resume
        if ((node.data.type == 9) || (node.data.type == 10)) {
            var nodeDataArray = model.nodeDataArray;
            var searchNodetype;
            if (node.data.type == 9) {
                searchNodetype = 10
            }
            if (node.data.type == 10) {
                searchNodetype = 9
            }
            var found_nodes = $.grep(nodeDataArray, function (curentnode) {
                return curentnode.group === group.data.key && curentnode.type == searchNodetype;
            });
            var removeableNode = node.containingGroup.diagram.findNodeForKey(found_nodes[0].key);
            myDig.remove(removeableNode);
        }

        //

        if (group != null && group.memberParts.count === 0) {
            myDig.remove(group);
        }

        self.showInspectorHeader();
    };

    this.deleteSelectedObject = function () {
        debugger;

        myDig.commandHandler.deleteSelection();
        self.showInspectorHeader();
    }

    this.graph.addDiagramListener("SelectionDeleting", function (e, obj) {
        debugger;
        var node = e.subject.first();
        var data = node.data;
        if (data.text) {
            if (!confirm("Are you sure to delete the link from the workflow?"))
                e.cancel = true;
        }
        if (typeof data.from != 'undefined') {

            if (data.to == data.from) {
                if (!confirm("Are you sure to delete the self loop?"))
                    e.cancel = true;
            }
        }

    });

    this.graph.addDiagramListener("SelectionDeleted", function (e, obj) {
        self.showInspectorHeader();
        var node = e.subject.first();
        self.deleteSegment(e, node);
    });

    this.addElement = function (eobj) {

        this.graph.nodeTemplateMap.add("Default", this.diagram(go.Node, "Auto", nodeStyle,
             new go.Binding("location", "loc", go.Point.parse),
                      this.diagram(go.Shape, "RoundedRectangle",
                       { width: 120, height: 40 },
                          new go.Binding("fill", "background"),
                          new go.Binding("stroke", "stroke")),

                          this.diagram(go.Panel, "Horizontal", { alignment: go.Spot.Left },
                          this.diagram(go.Shape, { margin: new go.Margin(3, 3, 3, 10), width: 20, height: 20, fill: "#F5F5F5", strokeWidth: 0 },
                        new go.Binding("geometry", "geo", geoFunc), new go.Binding("fill", "#F5F5F5"),
                        new go.Binding("angle", "angle")),
                        this.diagram(go.TextBlock, { margin: new go.Margin(0, 0, 0, 2), stroke: "#F5F5F5", font: currentFont },
                        new go.Binding("text", "text"))),
                     {
                         toolTip:
                         this.diagram(go.Adornment, "Auto",
                         this.diagram(go.Shape, { fill: "LightYellow", stroke: "#888", strokeWidth: 2 }),
                         this.diagram(go.TextBlock, { margin: 8, stroke: "#888", font: currentFont },
                        new go.Binding("text", "text"),
                        new go.Binding("stroke", "color"),
                        new go.Binding("touchPointId", "tpId")))
                     },
                          this.diagram("CloseButton",
                            { click: this.deleteSegmentwithConfirm }),
                        makePort(this, "T", go.Spot.Top, false, true),
                        makePort(this, "B", go.Spot.Bottom, true, false)
                  ));


        this.graph.nodeTemplateMap.add("Start",
                  this.diagram(go.Node, "Auto", nodeStyle,
                            new go.Binding("location", "loc", go.Point.parse),
                      this.diagram(go.Shape, "RoundedRectangle",
                       { width: 120, height: 40 },
                          new go.Binding("fill", "background"),
                          new go.Binding("stroke", "stroke")),
                          this.diagram(go.TextBlock, { margin: 8, stroke: "#F5F5F5", font: currentFont },
                    new go.Binding("text", "text")),
                      this.diagram(go.Shape,
                   { margin: new go.Margin(3, 3, 3, 10), alignment: go.Spot.Left, width: 20, height: 20, strokeWidth: 1, fill: "#F5F5F5", strokeWidth: 0 },
                      new go.Binding("geometry", "geo", geoFunc), new go.Binding("fill", "#F5F5F5"), new go.Binding("angle", "angle")),
                     {
                         toolTip:
                         this.diagram(go.Adornment, "Auto",
                         this.diagram(go.Shape, { fill: "LightYellow", stroke: "#888", strokeWidth: 2 }),
                         this.diagram(go.TextBlock, { margin: 8, stroke: "#888", font: currentFont },
                        new go.Binding("text", "text"),
                        new go.Binding("stroke", "color"),
                        new go.Binding("touchPointId", "tpId")))
                     },

                this.diagram("CloseButton",
                {
                    click: this.deleteSegmentwithConfirm
                }), makePort(this, "B", go.Spot.Bottom, true, false)));

        this.graph.nodeTemplateMap.add("End",
                    this.diagram(go.Node, "Auto", nodeStyle,
                            new go.Binding("location", "loc", go.Point.parse),
                      this.diagram(go.Shape, "RoundedRectangle",
                        { width: 120, height: 40 },
                          new go.Binding("fill", "background"),
                          new go.Binding("stroke", "#F5F5F5")),
                          this.diagram(go.TextBlock, { margin: 8, stroke: "#F5F5F5", font: currentFont },
                    new go.Binding("text", "text")),
                      this.diagram(go.Shape,
                                    { margin: new go.Margin(3, 3, 3, 10), alignment: go.Spot.Left, width: 20, height: 20, strokeWidth: 1, fill: "#F5F5F5", strokeWidth: 0 },
                      new go.Binding("geometry", "geo", geoFunc), new go.Binding("fill", "#F5F5F5")),
                     {
                         toolTip:
                         this.diagram(go.Adornment, "Auto",
                         this.diagram(go.Shape, { fill: "LightYellow", stroke: "#888", strokeWidth: 2 }),
                         this.diagram(go.TextBlock, { margin: 8, stroke: "#888", font: currentFont },
                        new go.Binding("text", "text"),
                        new go.Binding("stroke", "color"),
                        new go.Binding("touchPointId", "tpId")))
                     },

                         this.diagram("CloseButton",
                    { click: this.deleteSegmentwithConfirm }),
                   makePort(this, "T", go.Spot.Top, false, true)
                ));
    };
    this.graph.groupTemplate = this.diagram(go.Group, "Auto", nodeStyle,
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            new go.Binding("isSubGraphExpanded").makeTwoWay(),
       {

           layout: this.diagram(go.LayeredDigraphLayout,
                     {
                         direction: 0, columnSpacing: 10,
                         isOngoing: false,

                     }),
           mouseDragEnter: function (e, grp, prev) { highlightGroup(e, grp, true); },
           mouseDragLeave: function (e, grp, next) { highlightGroup(e, grp, false); },
           computesBoundsAfterDrag: false,
           mouseDrop: finishDrop,
           handlesDragDropForMembers: false
       },
      this.diagram(go.Shape, "RoundedRectangle", // surrounds everything
       new go.Binding("background", "isHighlighted", function (h)
       { return h ? "rgba(255,0,0,0.2)" : "transparent"; }).ofObject(),
                {
                    parameter1: 5,
                    fill: "rgba(128,128,128,0.33)",
                    strokeWidth: 1,
                    stroke: "#6F6F6F"
                }
        ), this.diagram("CloseButton",
                    { click: this.deleteSegmentwithConfirm }),
            makePort(this, "T", go.Spot.Top, false, true),
        this.diagram(go.Panel, "Vertical",  // position header above the subgraph
            { defaultAlignment: go.Spot.TopLeft },
        this.diagram(go.Panel, "Horizontal",  // the header
          { defaultAlignment: go.Spot.Top },
          this.diagram(go.TextBlock,     // group title near top, next to button
            { font: boldFont, alignment: go.Spot.Left, margin: new go.Margin(7, 0, 10, 10) },
            new go.Binding("text", "text")),
          this.diagram("SubGraphExpanderButton", { margin: new go.Margin(5, 3, 9, 5), alignment: go.Spot.Right })  // this Panel acts as a Button
         ),
        this.diagram(go.Placeholder,     // represents area for all member parts
          { padding: new go.Margin(0, 10), background: "transparent" }),
         makePort(this, "B", go.Spot.Bottom, true, false)
      )
    );

    this.addElement();

    this.graph.linkTemplate = this.diagram(go.Link,
      {
          routing: go.Link.AvoidsNodes,  // may be either Orthogonal or AvoidsNodes
          curve: go.Link.JumpOver,
          corner: 5, toShortLength: 4,
          relinkableFrom: true,
          relinkableTo: true,
          reshapable: true,
          resegmentable: true

      },
     this.diagram(go.Shape),
      this.diagram(go.Shape, { toArrow: "Standard" })
    );
    this.graph.toolManager.linkingTool.temporaryLink.routing = go.Link.AvoidsNodes;
    this.graph.toolManager.relinkingTool.temporaryLink.routing = go.Link.AvoidsNodes;

    this.addLink = function (data) {
        model.addLinkData(data);
    }

    this.addLinks = function (datas) {
        for (var i = 0; i < datas.length; i++)
            this.addLink(datas[i]);
    }

    this.addData = function (data) {
        model.addNodeData(data);
    };
    this.addDatas = function (datas) {

        for (var i = 0; i < datas.length; i++)
            this.addData(datas[i]);
    };
    this.removeData = function (data) {
        model.removeNodeData(data);
    };
    this.removeLinkData = function (data) {
        model.removeLinkData(data);
    };
    this.Palette =
     this.diagram(go.Palette, obj.palleteId,
       {

           "animationManager.duration": 800
       });

    this.Palette.layout.sorting = go.GridLayout.Forward;
    this.Palette.layout.wrappingColumn = 1;
    var templateDiagram = this.diagram(go.Node, "Horizontal",
        {
            defaultAlignment: go.Spot.Left,
            stretch: go.GraphObject.Horizontal,
            resizable: true,
            resizeObjectName: "SHAPE",
            mouseEnter: function (e, node) { node.findObject("SHAPE").fill = '#ccc'; },
            mouseLeave: function (e, node) { node.findObject("SHAPE").fill = '#FAFAFA'; }
        },
        this.diagram(go.Panel, "Auto",
                this.diagram(go.Shape, "Rectangle", { name: "SHAPE", fill: "#FAFAFA", strokeWidth: 0, height: 30 }),
                        new go.Binding('width', 'width'),
                this.diagram(go.Panel, "Horizontal", { alignment: go.Spot.Left },
                    this.diagram(go.Shape, { margin: new go.Margin(3, 3, 3, 10), width: 20, height: 20, fill: "#6F6F6F", strokeWidth: 0 },
                        new go.Binding("geometry", "geo", geoFunc), new go.Binding("fill", "#F5F5F5"), new go.Binding("angle", "angle")),
                        this.diagram(go.TextBlock, { margin: new go.Margin(0, 0, 0, 2), font: currentFont },
                        new go.Binding("text", "text")))
                    ),
           {
               toolTip:
               this.diagram(go.Adornment, "Auto",
               this.diagram(go.Shape, { fill: "LightYellow", stroke: "#888", strokeWidth: 2 }),
               this.diagram(go.TextBlock, { margin: 8, stroke: "#888", font: currentFont },
                   new go.Binding("text", "text"), new go.Binding("stroke", "color")))
           });





    this.Palette.nodeTemplateMap.add("Default", templateDiagram);
    this.Palette.nodeTemplateMap.add("Start", templateDiagram);
    this.Palette.nodeTemplateMap.add("End", templateDiagram);


    this.addPaletteData = function (data) {
        this.Palette.model.addNodeData(data);
    };

    this.setGridLines = function (setToTrue) {
        this.graph.grid.visible = setToTrue;
        this.graph.toolManager.draggingTool.isGridSnapEnabled = setToTrue;
        this.graph.toolManager.resizingTool.isGridSnapEnabled = setToTrue;
    };

    function geoFunc(geoname) {
        debugger;
        var geo = icons[geoname];
        if (geo === undefined) geo = "start";  // use this for an unknown icon name
        if (typeof geo === "string") {
            debugger;
            geo = icons[geoname] = go.Geometry.parse(geo, true);  // fill each geometry
        }
        return geo;
    }
    this.addPaletteDatas = function (datas) {

        for (var i = 0; i < datas.length; i++)
            this.addPaletteData(datas[i]);
    };

    this.addGeneralPaletteDatas = function (datas) {

        this.GeneralPalette =
    this.diagram(go.Palette, "myPaletteGeneral",  // must name or refer to the DIV HTML element
      {

          "animationManager.duration": 800 // slightly longer than default (600ms) animation
      });
        this.GeneralPalette.layout.wrappingColumn = 1;
        this.GeneralPalette.layout.sorting = go.GridLayout.Forward;
        this.GeneralPalette.nodeTemplateMap.add("Default", templateDiagram);
        this.GeneralPalette.nodeTemplateMap.add("Start", templateDiagram);
        this.GeneralPalette.nodeTemplateMap.add("End", templateDiagram);

        for (var i = 0; i < datas.length; i++) {
            this.GeneralPalette.model.addNodeData(datas[i]);
        }

    };


    this.addTemplatePaletteDatas = function (datas) {
        this.TemplatePalette =
    this.diagram(go.Palette, "myPaletteTemplate",  // must name or refer to the DIV HTML element
      {

          "animationManager.duration": 800 // slightly longer than default (600ms) animation
      });
        this.TemplatePalette.layout.wrappingColumn = 1;
        this.TemplatePalette.layout.sorting = go.GridLayout.Forward;
        this.TemplatePalette.nodeTemplateMap.add("Default", templateDiagram);
        this.TemplatePalette.nodeTemplateMap.add("Start", templateDiagram);
        this.TemplatePalette.nodeTemplateMap.add("End", templateDiagram);

        for (var i = 0; i < datas.length; i++) {
            this.TemplatePalette.model.addNodeData(datas[i]);
        }

    };


    this.addStartPaletteDatas = function (datas) {
        this.StartPalette =
    this.diagram(go.Palette, "myPaletteStart",  // must name or refer to the DIV HTML element
      {

          "animationManager.duration": 800 // slightly longer than default (600ms) animation
      });
        this.StartPalette.layout.wrappingColumn = 1;
        this.StartPalette.layout.sorting = go.GridLayout.Forward;
        this.StartPalette.nodeTemplateMap.add("Default", templateDiagram);
        this.StartPalette.nodeTemplateMap.add("Start", templateDiagram);
        this.StartPalette.nodeTemplateMap.add("End", templateDiagram);

        for (var i = 0; i < datas.length; i++) {
            this.StartPalette.model.addNodeData(datas[i]);
        }

    };

    this.addSearchPaletteDatas = function (datas) {

        //this.diagram.div('myPaletteSearch') = null;
        var searchDiv = this.SearchPalette;
        if (this.SearchPalette == null) {
            this.SearchPalette =
   this.diagram(go.Palette, "myPaletteSearch",  // must name or refer to the DIV HTML element
     {

         "animationManager.duration": 800 // slightly longer than default (600ms) animation
     });
            this.SearchPalette.layout.wrappingColumn = 1;
            this.SearchPalette.layout.sorting = go.GridLayout.Forward;
            this.SearchPalette.nodeTemplateMap.add("Default", templateDiagram);
            this.SearchPalette.nodeTemplateMap.add("Start", templateDiagram);
            this.SearchPalette.nodeTemplateMap.add("End", templateDiagram);
        }
        else {
            this.SearchPalette.model.nodeDataArray = [];
        }
        for (var i = 0; i < datas.length; i++) {
            this.SearchPalette.model.addNodeData(datas[i]);
        }

    };


    //new Output Processed Touchpoint
    this.AddOutputProccessedTemplate = function (key, id, val, cid) {

        var key1 = generateUUID();
        var key2 = generateUUID();
        var data = model.nodeDataArray[model.nodeDataArray.length - 1]
        this.setModelData(data, "Process", "text");
        this.setModelData(data, id, "touchPointId");
        this.setModelData(data, cid, "connectionId");
        this.setModelData(data, key, "key");
        this.setModelData(data, "process", "geo");
        this.setModelData(data, "#888", "stroke");
        this.setModelData(data, "#1BBC9B", "background");
        this.setModelData(data, "#364150", "color");
        this.setModelData(data, key1, "group");
        this.setModelData(data, "Default", "category");


        this.addDatas([this.getPost(id, key2, key1, cid), this.getGroup(id, key1, val, cid)]);
        debugger;
        this.addLinks([{ key: generateUUID(), from: key, to: key2, text: '2', ExecutionOrder: 1 }]);
    };

    //new GetAggregated touchpoint
    this.AddGetMergeTemplate = function (key, id, val, entity, path, batchsize, cid) {
        var key1 = generateUUID();
        var key2 = generateUUID();
        var key3 = generateUUID();
        var key4 = generateUUID();
        var data = model.nodeDataArray[model.nodeDataArray.length - 1]
        model.startTransaction("changeName");
        model.setDataProperty(data, "text", "Get");
        model.commitTransaction("changeName");

        model.startTransaction("changeId");
        model.setDataProperty(data, "touchPointId", id);
        model.commitTransaction("changeId");

        model.startTransaction("changecId");
        model.setDataProperty(data, "connectionId", cid);
        model.commitTransaction("changecId");

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", key);
        model.commitTransaction("changeUid");

        model.startTransaction("changeType");
        model.setDataProperty(data, "type", 5);
        model.commitTransaction("changeType");

        model.startTransaction("changegeo");
        model.setDataProperty(data, "geo", "get");
        model.commitTransaction("changegeo");

        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#32C5D2");
        model.commitTransaction("changeBackground");

        model.startTransaction("changeColor");
        model.setDataProperty(data, "color", "#364150");
        model.commitTransaction("changeColor");

        model.startTransaction("changegrp");
        model.setDataProperty(data, "group", key1);
        model.commitTransaction("changegrp");

        model.startTransaction("changecategory");
        model.setDataProperty(data, "category", "Default");
        model.commitTransaction("changecategory");

        model.startTransaction("changeangle");
        model.setDataProperty(data, "angle", 180);
        model.commitTransaction("changeangle");

        this.addDatas([{
            key: key2,
            text: "Merger",
            stroke: "#888",
            background: "#8775A7",
            color: "#364150",
            category: "Default",
            touchPointId: id,
            connectionId: cid,
            geo: "merger",
            group: key1,
            type: 2,
            Entity: entity,
            Xpath: path,
            BatchSize: batchsize,
            //loc: "250 0"

        }, {
            key: key3,
            text: "Process",
            stroke: "#888",
            background: "#1BBC9B",
            color: "#364150",
            category: "Default",
            geo: "process",
            touchPointId: id,
            connectionId: cid,
            group: key1,
            type: 7,
            //loc: "450 0"

        }, {
            key: key4,
            text: "Post",
            stroke: "#888",
            background: "#4B77BE",
            color: "#364150",
            category: "Default",
            geo: "post",
            touchPointId: id,
            connectionId: cid,
            angle: 180,
            group: key1,
            type: 6,
            //loc: "650 0"

        }, {
            key: key1,
            touchPointId: id,
            connectionId: cid,
            isGroup: true,
            text: val,
            type: 1
        }]);
        this.addLinks([{ key: generateUUID(), from: key, to: key2, text: '2', ExecutionOrder: 1 }, { key: generateUUID(), from: key2, to: key3, text: '2', ExecutionOrder: 1 }, { key: generateUUID(), from: key3, to: key4, text: '2', ExecutionOrder: 1 }]);


    };

    // new Post Aggregated Touchpoint 
    this.AddPostMergeTemplate = function (key, id, val, entity, path, batchsize, cid) {


        var key1 = generateUUID();
        var key2 = generateUUID();
        var key3 = generateUUID();
        var key4 = generateUUID();
        var data = model.nodeDataArray[model.nodeDataArray.length - 1];
        model.startTransaction("changeName");
        model.setDataProperty(data, "text", "Get");
        model.commitTransaction("changeName");

        model.startTransaction("changeId");
        model.setDataProperty(data, "touchPointId", id);
        model.commitTransaction("changeId");

        model.startTransaction("changecId");
        model.setDataProperty(data, "connectionId", cid);
        model.commitTransaction("changecId");

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", key);
        model.commitTransaction("changeUid");

        model.startTransaction("changeType");
        model.setDataProperty(data, "type", 5);
        model.commitTransaction("changeType");

        model.startTransaction("changegeo");
        model.setDataProperty(data, "geo", "get");
        model.commitTransaction("changegeo");

        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#32C5D2");
        model.commitTransaction("changeBackground");

        model.startTransaction("changeColor");
        model.setDataProperty(data, "color", "#364150");
        model.commitTransaction("changeColor");

        model.startTransaction("changegrp");
        model.setDataProperty(data, "group", key1);
        model.commitTransaction("changegrp");

        model.startTransaction("changecategory");
        model.setDataProperty(data, "category", "Default");
        model.commitTransaction("changecategory");

        //model.startTransaction("changelocation");
        //model.setDataProperty(data, "loc", "50 0");
        //model.commitTransaction("changelocation");
        model.startTransaction("changeangle");
        model.setDataProperty(data, "angle", 180);
        model.commitTransaction("changeangle");


        this.addDatas([{
            key: key2,
            text: "Process",
            stroke: "#888",
            background: "#1BBC9B",
            color: "#364150",
            category: "Default",
            geo: "process",
            touchPointId: id,
            connectionId: cid,
            group: key1,
            type: 7,
            //loc: "250 0"

        }, {
            key: key3,
            text: "Merger",
            stroke: "#888",
            background: "#8775A7",
            color: "#364150",
            category: "Default",
            touchPointId: id,
            connectionId: cid,
            geo: "merger",
            group: key1,
            type: 2,
            Entity: entity,
            Xpath: path,
            BatchSize: batchsize,
            //loc: "450 0"

        }, {
            key: key4,
            text: "Post",
            stroke: "#888",
            background: "#4B77BE",
            color: "#364150",
            category: "Default",
            geo: "post",
            touchPointId: id,
            connectionId: cid,
            angle: 180,
            group: key1,
            type: 6,
            //loc: "650 0"

        }, {
            key: key1,
            touchPointId: id,
            connectionId: cid,
            isGroup: true,
            text: val,
            type: 1
        }]);
        this.addLinks([{ key: generateUUID(), from: key, to: key2, text: '2', ExecutionOrder: 1 }, { key: generateUUID(), from: key2, to: key3, text: '2', ExecutionOrder: 1 }, { key: generateUUID(), from: key3, to: key4, text: '2', ExecutionOrder: 1 }]);

    },
    // new Get Splitted Touchpoint

    this.AddGetSplittedTemplate = function (key, id, val, entity, path, batchsize, cid) {

        var key1 = generateUUID();
        var key2 = generateUUID();
        var key3 = generateUUID();
        var key4 = generateUUID();
        var data = model.nodeDataArray[model.nodeDataArray.length - 1]
        model.startTransaction("changeName");
        model.setDataProperty(data, "text", "Get");
        model.commitTransaction("changeName");

        model.startTransaction("changeId");
        model.setDataProperty(data, "touchPointId", id);
        model.commitTransaction("changeId");

        model.startTransaction("changecId");
        model.setDataProperty(data, "connectionId", cid);
        model.commitTransaction("changecId");

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", key);
        model.commitTransaction("changeUid");

        model.startTransaction("changeType");
        model.setDataProperty(data, "type", 5);
        model.commitTransaction("changeType");

        model.startTransaction("changegeo");
        model.setDataProperty(data, "geo", "get");
        model.commitTransaction("changegeo");


        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#32C5D2");
        model.commitTransaction("changeBackground");

        model.startTransaction("changeColor");
        model.setDataProperty(data, "color", "#364150");
        model.commitTransaction("changeColor");

        model.startTransaction("changegrp");
        model.setDataProperty(data, "group", key1);
        model.commitTransaction("changegrp");

        model.startTransaction("changecategory");
        model.setDataProperty(data, "category", "Default");
        model.commitTransaction("changecategory");

        //model.startTransaction("changelocation");
        //model.setDataProperty(data, "loc", "50 0");
        //model.commitTransaction("changelocation");
        model.startTransaction("changeangle");
        model.setDataProperty(data, "angle", 180);
        model.commitTransaction("changeangle");

        this.addDatas([{
            key: key2,
            text: "Split",
            stroke: "#888",
            background: "#9B59B6",
            color: "#364150",
            category: "Default",
            geo: "split",
            touchPointId: id,
            connectionId: cid,
            group: key1,
            type: 3,
            Entity: entity,
            Xpath: path,
            BatchSize: batchsize,
            //loc: "250 0"


        }, {
            key: key3,
            text: "Process",
            stroke: "#888",
            background: "#1BBC9B",
            color: "#364150",
            category: "Default",
            geo: "process",
            touchPointId: id,
            connectionId: cid,
            group: key1,
            type: 7,
            //loc: "450 0"
        }, {
            key: key4,
            text: "Post",
            stroke: "#888",
            background: "#4B77BE",
            color: "#364150",
            category: "Default",
            geo: "post",
            touchPointId: id,
            connectionId: cid,
            angle: 180,
            group: key1,
            type: 6,
            //loc: "650 0"
        }, {
            key: key1,
            touchPointId: id,
            connectionId: cid,
            isGroup: true,
            text: val,
            type: 1
        }]);
        this.addLinks([{ key: generateUUID(), from: key, to: key2, text: '2', ExecutionOrder: 1 }, { key: generateUUID(), from: key2, to: key3, text: '2', ExecutionOrder: 1 }, { key: generateUUID(), from: key3, to: key4, text: '2', ExecutionOrder: 1 }]);

    },

    //new Post Aggregated Touchpoint

    this.AddPostSplittedTemplate = function (key, id, val, entity, path, batchsize, cid) {
        var key1 = generateUUID();
        var key2 = generateUUID();
        var key3 = generateUUID();
        var key4 = generateUUID();
        var data = model.nodeDataArray[model.nodeDataArray.length - 1];

        model.startTransaction("changeName");
        model.setDataProperty(data, "text", "Get");
        model.commitTransaction("changeName");

        model.startTransaction("changeId");
        model.setDataProperty(data, "touchPointId", id);
        model.commitTransaction("changeId");

        model.startTransaction("changecId");
        model.setDataProperty(data, "connectionId", cid);
        model.commitTransaction("changecId");

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", key);
        model.commitTransaction("changeUid");

        model.startTransaction("changeType");
        model.setDataProperty(data, "type", 5);
        model.commitTransaction("changeType");

        model.startTransaction("changegeo");
        model.setDataProperty(data, "geo", "get");
        model.commitTransaction("changegeo");

        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#32C5D2");
        model.commitTransaction("changeBackground");

        model.startTransaction("changeColor");
        model.setDataProperty(data, "color", "#364150");
        model.commitTransaction("changeColor");

        model.startTransaction("changegrp");
        model.setDataProperty(data, "group", key1);
        model.commitTransaction("changegrp");

        model.startTransaction("changecategory");
        model.setDataProperty(data, "category", "Default");
        model.commitTransaction("changecategory");

        //model.startTransaction("changelocation");
        //model.setDataProperty(data, "loc", "50 0");
        //model.commitTransaction("changelocation");
        model.startTransaction("changeangle");
        model.setDataProperty(data, "angle", 180);
        model.commitTransaction("changeangle");

        this.addDatas([{
            key: key2,
            text: "Process",
            stroke: "#888",
            background: "#1BBC9B",
            color: "#364150",
            category: "Default",
            geo: "process",
            touchPointId: id,
            connectionId: cid,
            group: key1,
            type: 7,
            //loc: "250 0"
        }, {
            key: key3,
            text: "Split",
            stroke: "#888",
            background: "#9B59B6",
            color: "#364150",
            category: "Default",
            geo: "split",
            touchPointId: id,
            connectionId: cid,
            group: key1,
            type: 3,
            Entity: entity,
            Xpath: path,
            BatchSize: batchsize,
            //loc: "450 0"

        }, {
            key: key4,
            text: "Post",
            stroke: "#888",
            background: "#4B77BE",
            color: "#364150",
            category: "Default",
            geo: "post",
            touchPointId: id,
            connectionId: cid,
            angle: 180,
            group: key1,
            type: 6,
            //loc: "650 0"
        }, {
            key: key1,
            touchPointId: id,
            connectionId: cid,
            isGroup: true,
            text: val,
            type: 1
        }]);

        this.addLinks([{ key: generateUUID(), from: key, to: key2, text: '2', ExecutionOrder: 1 }, { key: generateUUID(), from: key2, to: key3, text: '2', ExecutionOrder: 1 }, { key: generateUUID(), from: key3, to: key4, text: '2', ExecutionOrder: 1 }]);

    };

    this.AddNodeGroupTemplate = function (key, id, val, cid) {
        var key1 = generateUUID();
        var key2 = generateUUID();
        var key3 = generateUUID();
        var data = model.nodeDataArray[model.nodeDataArray.length - 1];

        model.startTransaction("changeName");
        model.setDataProperty(data, "text", "Get");
        model.commitTransaction("changeName");

        model.startTransaction("changeId");
        model.setDataProperty(data, "touchPointId", id);
        model.commitTransaction("changeId");

        model.startTransaction("changecId");
        model.setDataProperty(data, "connectionId", cid);
        model.commitTransaction("changecId");

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", key);
        model.commitTransaction("changeUid");

        model.startTransaction("changeType");
        model.setDataProperty(data, "type", 5);
        model.commitTransaction("changeType");

        model.startTransaction("changegeo");
        model.setDataProperty(data, "geo", "get");
        model.commitTransaction("changegeo");

        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#32C5D2");
        model.commitTransaction("changeBackground");

        model.startTransaction("changeColor");
        model.setDataProperty(data, "color", "#364150");
        model.commitTransaction("changeColor");

        model.startTransaction("changegrp");
        model.setDataProperty(data, "group", key1);
        model.commitTransaction("changegrp");

        model.startTransaction("changecategory");
        model.setDataProperty(data, "category", "Default");
        model.commitTransaction("changecategory");

        //model.startTransaction("changelocation");
        //model.setDataProperty(data, "loc", "0 0");
        //model.commitTransaction("changelocation");
        model.startTransaction("changeangle");
        model.setDataProperty(data, "angle", 180);
        model.commitTransaction("changeangle");
        this.addDatas([{
            key: key2,
            text: "Process",
            stroke: "#888",
            background: "#1BBC9B",
            color: "#364150",
            category: "Default",
            touchPointId: id,
            connectionId: cid,
            geo: "process",
            group: key1,
            type: 7,
            //loc: "50 0"

        }, {
            key: key3,
            text: "Post",
            stroke: "#888",
            background: "#4B77BE",
            color: "#364150",
            category: "Default",
            touchPointId: id,
            connectionId: cid,
            geo: "post",
            group: key1,
            angle: 180,
            type: 6,
            //loc: "100 0"

        }, {
            key: key1,
            touchPointId: id,
            connectionId: cid,
            isGroup: true,
            text: val,
            type: 1
        }]);



        this.addLinks([{ key: generateUUID(), from: key, to: key2, text: '2', ExecutionOrder: 1 }, { key: generateUUID(), from: key2, to: key3, text: '2', ExecutionOrder: 1 }]);
    };


    this.AddNodeTemplate = function (key, id, val, type, txt, shape, cid, angle) {
        var key1 = generateUUID();

        var data = model.nodeDataArray[model.nodeDataArray.length - 1];

        model.startTransaction("changeName");
        model.setDataProperty(data, "text", txt);
        model.commitTransaction("changeName");

        model.startTransaction("changeId");
        model.setDataProperty(data, "touchPointId", id);
        model.commitTransaction("changeId");

        model.startTransaction("changecId");
        model.setDataProperty(data, "connectionId", cid);
        model.commitTransaction("changecId");

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", key1);
        model.commitTransaction("changeUid");

        model.startTransaction("changeType");
        model.setDataProperty(data, "type", type);
        model.commitTransaction("changeType");

        model.startTransaction("changegeo");
        model.setDataProperty(data, "geo", shape);
        model.commitTransaction("changegeo");

        model.startTransaction("changegrp");
        model.setDataProperty(data, "group", key);
        model.commitTransaction("changegrp");

        model.startTransaction("changecategory");
        model.setDataProperty(data, "category", "Default");
        model.commitTransaction("changecategory");

        model.startTransaction("changeangle");
        model.setDataProperty(data, "angle", angle);
        model.commitTransaction("changeangle");


        this.addDatas([{
            key: key,
            touchPointId: id,
            connectionId: cid,
            isGroup: true,
            text: val,
            type: 1
        }]);



    };




    this.ComponentsDataTemplate = function (key, id, val, type, txt, shape, entity, path, batchsize, cid) {
        var key1 = generateUUID();

        var data = model.nodeDataArray[model.nodeDataArray.length - 1];

        model.startTransaction("changeName");
        model.setDataProperty(data, "text", txt);
        model.commitTransaction("changeName");

        model.startTransaction("changeId");
        model.setDataProperty(data, "touchPointId", id);
        model.commitTransaction("changeId");

        model.startTransaction("changecId");
        model.setDataProperty(data, "connectionId", cid);
        model.commitTransaction("changecId");

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", key1);
        model.commitTransaction("changeUid");

        model.startTransaction("changeType");
        model.setDataProperty(data, "type", type);
        model.commitTransaction("changeType");

        model.startTransaction("changegeo");
        model.setDataProperty(data, "geo", shape);
        model.commitTransaction("changegeo");

        model.startTransaction("changegrp");
        model.setDataProperty(data, "group", key);
        model.commitTransaction("changegrp");

        model.startTransaction("changecategory");
        model.setDataProperty(data, "category", "Default");
        model.commitTransaction("changecategory");

        model.startTransaction("changeentity");
        model.setDataProperty(data, "Entity", entity);
        model.commitTransaction("changeentity");

        //model.startTransaction("changenamespace");
        //model.setDataProperty(data, "Namespace", name);
        //model.commitTransaction("changenamespace");

        model.startTransaction("changepath");
        model.setDataProperty(data, "Xpath", path);
        model.commitTransaction("changepath");

        model.startTransaction("changepbatchsize");
        model.setDataProperty(data, "BatchSize", batchsize);
        model.commitTransaction("changebatchsize");

        this.addDatas([{
            key: key,
            touchPointId: id,
            connectionId: cid,
            isGroup: true,
            text: val,
            type: 1
        }]);



    };

    this.delayTemplate = function (key, id, val, delaytime, cid) {


        var key1 = generateUUID();
        var data = model.nodeDataArray[model.nodeDataArray.length - 1];

        //this.setModelData(data, delaytime, "Delaytime");
        //this.setModelData(data, id, "touchPointId");
        //this.setModelData(data, key, "key");

        model.startTransaction("changeDelayTime");
        model.setDataProperty(data, "Delaytime", delaytime);
        model.commitTransaction("changeDelayTime");

        if (id != null) {
            model.startTransaction("changeId");
            model.setDataProperty(data, "touchPointId", id);
            model.commitTransaction("changeId");
        }
     
        model.startTransaction("changecId");
        model.setDataProperty(data, "connectionId", cid);
        model.commitTransaction("changecId");

        model.startTransaction("changeKey");
        model.setDataProperty(data, "key", key);
        model.commitTransaction("changeKey");

        if (val != null) {

            this.addDatas([this.getGroup(id, key1, val, cid)]);

            //this.setModelData(data, key1, "group");
            model.startTransaction("changeGroup");
            model.setDataProperty(data, "group", key1);
            model.commitTransaction("changeGroup");

        }


    };

    //This is for resume template

    this.addResumeTemplate = function (nodeid, key, id, cid, val) {

        var key1 = generateUUID();
        var data = model.nodeDataArray[model.nodeDataArray.length - 1];

        model.startTransaction("changeName");
        model.setDataProperty(data, "text", "Halt");
        model.commitTransaction("changeName");

        model.startTransaction("changeId");
        model.setDataProperty(data, "touchPointId", id);
        model.commitTransaction("changeId");

        model.startTransaction("changecId");
        model.setDataProperty(data, "connectionId", cid);
        model.commitTransaction("changecId");

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", nodeid);
        model.commitTransaction("changeUid");

        model.startTransaction("changeType");
        model.setDataProperty(data, "type", 10);
        model.commitTransaction("changeType");

        model.startTransaction("changegeo");
        model.setDataProperty(data, "geo", "halt");
        model.commitTransaction("changegeo");

        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#F2784B");
        model.commitTransaction("changeBackground");

        model.startTransaction("changeColor");
        model.setDataProperty(data, "color", "#364150");
        model.commitTransaction("changeColor");

        model.startTransaction("changegrp");
        model.setDataProperty(data, "group", key);
        model.commitTransaction("changegrp");

        model.startTransaction("changecategory");
        model.setDataProperty(data, "category", "Default");
        model.commitTransaction("changecategory");



        if (val != null) {

            this.addDatas([{
                key: key1,
                text: "Resume",
                stroke: "#888",
                background: "#F3C200",
                color: "#364150",
                category: "Default",
                geo: "resume",
                touchPointId: id,
                connectionId: cid,
                type: 9,
                group: data.group,
                loc: "0 0"

            }]);

        }
    }



    ////This is for resync template
    //this.resyncDataTemplate = function (key, id, val, type, txt, shape, scheduleRepeat, repeatInterval, endchk, endAfter, cid) {

    //    var key1 = generateUUID();
    //    var data = model.nodeDataArray[model.nodeDataArray.length - 1];

    //    model.startTransaction("changeName");
    //    model.setDataProperty(data, "text", txt);
    //    model.commitTransaction("changeName");

    //    model.startTransaction("changeId");
    //    model.setDataProperty(data, "touchPointId", id);
    //    model.commitTransaction("changeId");

    //    model.startTransaction("changecId");
    //    model.setDataProperty(data, "connectionId", cid);
    //    model.commitTransaction("changecId");

    //    model.startTransaction("changeUid");
    //    model.setDataProperty(data, "key", key1);
    //    model.commitTransaction("changeUid");

    //    model.startTransaction("changeType");
    //    model.setDataProperty(data, "type", type);
    //    model.commitTransaction("changeType");

    //    model.startTransaction("changegeo");
    //    model.setDataProperty(data, "geometryString", shape);
    //    model.commitTransaction("changegeo");

    //    model.startTransaction("changegrp");
    //    model.setDataProperty(data, "group", key);
    //    model.commitTransaction("changegrp");

    //    model.startTransaction("changecategory");
    //    model.setDataProperty(data, "category", "Default");
    //    model.commitTransaction("changecategory");

    //    model.startTransaction("changeScheduleRepeat");
    //    model.setDataProperty(data, "ScheduleRepeat", repeatInterval);
    //    model.commitTransaction("changeScheduleRepeat");

    //    model.startTransaction("changeRepeatInterval");
    //    model.setDataProperty(data, "RepeatInterval", scheduleRepeat);
    //    model.commitTransaction("changeRepeatInterval");

    //    model.startTransaction("changeEndCheck");
    //    model.setDataProperty(data, "EndCheck", endchk);
    //    model.commitTransaction("changeEndCheck");

    //    model.startTransaction("changeEndAfter");
    //    model.setDataProperty(data, "EndAfter", endAfter);
    //    model.commitTransaction("changeEndAfter");

    //    this.addDatas([{
    //        key: key,
    //        touchPointId: id,
    //        connectionId: cid,
    //        isGroup: true,
    //        text: val,
    //        type: 1
    //    }]);

    //};


    this.setModelData = function (obj, data, key) {

        model.startTransaction(key);
        model.setDataProperty(obj, key, data);
        model.commitTransaction(key);
    };

    this.getGroup = function (id, key, val, cid) {
        return {
            key: key,
            touchPointId: id,
            connectionId: cid,
            isGroup: true,
            text: val,
            type: 1
        };
    };
    this.getProcess = function () {

    };
    this.getPost = function (id, key, groupKey, cid) {
        return {
            key: key,
            text: "Post",
            stroke: "#888",
            background: "#4B77BE",
            color: "#364150",
            category: "Default",
            touchPointId: id,
            connectionId: cid,
            geo: "post",
            group: groupKey,
            type: 6,
            angle: 180
        };
    };


    this.DataTemplate = function (entity, path, id) {



        var data = model.nodeDataArray[model.nodeDataArray.length - 1];



        model.startTransaction("changeentity");
        model.setDataProperty(data, "Entity", entity);
        model.commitTransaction("changeentity");

        model.startTransaction("changepath");
        model.setDataProperty(data, "Xpath", path);
        model.commitTransaction("changepath");

        model.startTransaction("changepbatchsize");
        model.setDataProperty(data, "BatchSize", batchsize);
        model.commitTransaction("changebatchsize");

        model.startTransaction("changeId");
        model.setDataProperty(data, "touchPointId", id);
        model.commitTransaction("changeId");


    };
    //this.resyncTemplate = function (scheduleRepeat, repeatInterval, endchk, endAfter, id) {
    //
    //    var data = model.nodeDataArray[model.nodeDataArray.length - 1];

    //    model.startTransaction("changeScheduleRepeat");
    //    model.setDataProperty(data, "ScheduleRepeat", repeatInterval);
    //    model.commitTransaction("changeScheduleRepeat");

    //    model.startTransaction("changeRepeatInterval");
    //    model.setDataProperty(data, "RepeatInterval", scheduleRepeat);
    //    model.commitTransaction("changeRepeatInterval");

    //    model.startTransaction("changeEndCheck");
    //    model.setDataProperty(data, "EndCheck", endchk);
    //    model.commitTransaction("changeEndCheck");

    //    model.startTransaction("changeEndAfter");
    //    model.setDataProperty(data, "EndAfter", endAfter);
    //    model.commitTransaction("changeEndAfter");

    //    model.startTransaction("changeId");
    //    model.setDataProperty(data, "touchPointId", id);
    //    model.commitTransaction("changeId");

    //};

    this.assignNodeId = function (id) {



        var data = model.nodeDataArray[model.nodeDataArray.length - 1];


        model.startTransaction("changeId");
        model.setDataProperty(data, "touchPointId", id);
        model.commitTransaction("changeId");


    };
    this.changeDir = function (data) {
        if (!data) {
            data = model.linkDataArray[model.linkDataArray.length - 1];
        }
        var toData = data.to;
        var fromData = data.from;
        var array1 = model.nodeDataArray;
        var attr = "key";
        var value1 = fromData;
        var value2 = toData;
        var loc1, loc2, fx, fy, sx, sy;

        for (var i = 0; i < array1.length; i++) {
            if (array1[i][attr] === value1) {
                loc1 = array1[i].key;
            }
            if (array1[i][attr] === value2) {
                loc2 = array1[i].key;
            }
        }


    };

    this.getLinks = function (key) {

        var links = model.linkDataArray;
        var attr = "from";
        var searchval = key;
        var wflinks = [];
        for (var i = 0; i < links.length; i++) {
            if (links[i][attr] == searchval) {
                wflinks.push(links[i]);
            }
        }
        return wflinks;
    };

    this.changeLinkName = function (key, id, val) {

        var data = model.linkDataArray[model.linkDataArray.length - 1];  // get the first node data

        model.startTransaction("changeLinkName");
        model.setDataProperty(data, "text", val);
        model.commitTransaction("changeLinkName");

        model.startTransaction("changeLinkid");
        model.setDataProperty(data, "ExecutionOrder", id);
        model.commitTransaction("changeLinkid");

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", key);
        model.commitTransaction("changeUid");
    };

    this.changeSelfLoopLinkName = function (key, val, chk) {
        var data = model.linkDataArray[model.linkDataArray.length - 1];
        if (val != null) {
            model.startTransaction("changeLinkName");
            model.setDataProperty(data, "ExecutionTime", val);
            model.commitTransaction("changeLinkName");
        }

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", key);
        model.commitTransaction("changeUid");

        model.startTransaction("changechk");
        model.setDataProperty(data, "ExecutionChk", chk);
        model.commitTransaction("changechk");
    };


    this.getModelJson = function () {

        return model.toJson();

    };


    this.getCurrentNode = function () {
        return model.nodeDataArray[model.nodeDataArray.length - 1];
    }

    this.getSelectedNode = function () {
        var selectednode = this.graph.selection.first();
        return this.graph.selection.first();
    }

    this.getSelectedLink = function () {
        var selectednode = this.graph.selection.first();
        return this.graph.selection.first();
    }

    this.getCurrentlink = function () {
        return model.linkDataArray[model.linkDataArray.length - 1];
    }
    var inspector = null;
    var validateResult = function (data) {
        if (data) {
            if (typeof data['type'] !== 'undefined')
                return data['type'] > 0;

            if (typeof data['from'] !== 'undefined')
                return data['from'] == data['to'];
        }
        return false;
    };
    this.showInspectorHeader = function (e, u) {

        if (e) {
            //$('#con-details').show();
            var data = (e instanceof go.Part) ? e.data : e;
            var taskId = data["touchPointId"];
            var connectionId = data["connectionId"];
            //for self loop
            if (typeof data["fromPort"] != 'undefined' && data["from"] == data["to"]) {
                var toNode = getNode(model.nodeDataArray, data["to"]);
                taskId = toNode.touchPointId;
                connectionId = toNode.connectionId;
            }
            //for self loop

            WorkFlow.findByTaskId(taskId, connectionId);

            var result = validateResult(data)
            //$('.theme-panel').toggle(result)
            if (typeof u !== 'undefined') {
                data['isDirty'] = true;
                inspector.inspectObject(e);
                delete data.isDirty;
            }

            if ((data.type != '8') && (typeof data.touchPointId == 'undefined') && (data["fromPort"] == 'undefined')) {
                var divs = $('#condetails,#nodedetails,#txtConDesc,#Connectiondetails');
                divs.hide();
                var workflowdiv = $('#workflowdetails');
                workflowdiv.show();
            }
            else if ((data.type == '-1') || (data.type == '0')) {
                var divs = $('#condetails,#nodedetails,#txtConDesc,#Connectiondetails,#linkHeading,#getdetails,#noSelectiondetails,#touchpointdetails,#processdetails');
                divs.hide();
                var workflowdiv = $('#workflowdetails');
                workflowdiv.show();
                var actionSpan = $('#actions').css('display', 'none');
            }
            else {
                var actionSpan = $('#actions').css('display', 'none');
                var workflowdiv = $('#workflowdetails,#getdetails,#noSelectiondetails,#touchpointdetails,#processdetails');
                workflowdiv.hide();
                var divs = $('#nodedetails,#txtConDesc,#Connectiondetails');
                var condiv = $('#condetails');
                divs.show();
                if (data.isGroup == true) {
                    var actionSpan = $('#actions').css('display', 'block');
                    var touchpointdiv = $('#touchpointdetails');
                    var div_hide = $('#processdetails,#noSelectiondetails,#getdetails');
                    touchpointdiv.show();
                    div_hide.hide();
                }
                if (data.type == 5) {
                    var actionSpan = $('#actions').css('display', 'block');
                    var getdiv = $('#getdetails');
                    var div_hide = $('#processdetails,#noSelectiondetails,#touchpointdetails');
                    getdiv.show();
                    div_hide.hide();
                }
                if (data.type == 7) {
                    var actionSpan = $('#actions').css('display', 'block');
                    var processdiv = $('#processdetails');
                    var div_hide = $('#getdetails,#noSelectiondetails,#touchpointdetails');
                    processdiv.show();
                    div_hide.hide();
                }
                if ((data.type == 6) || (data.type == 2) || (data.type == 3) || (data.type == 10) || (data.type == 8)) {
                    var div_hide = $('#getdetails,#noSelectiondetails,#touchpointdetails,#processdetails');
                    div_hide.hide();
                }

                if ((data.type == '8') || (typeof data["fromPort"] != 'undefined')) {
                    var toPort = data["to"];
                    var toNode = getNode(model.nodeDataArray, toPort);
                    var fromNode = getNode(model.nodeDataArray, data["from"]);
                    if ((data.type != '8') && ((fromNode.type == "0") || (toNode.type == "-1"))) {
                        $('#nodedetails,#condetails,#txtConDesc,#Connectiondetails').hide();
                        $('#workflowdetails').show();
                        $('#linkHeading').hide();
                    }
                    else if ((data.type != '8') && (toNode.type == "8")) {
                        if (typeof data["fromPort"] != 'undefined' && typeof fromNode.isGroup == 'undefined') {
                            var fromNode = getNode(model.nodeDataArray, data["from"]);
                            $('#nodedetails').show();
                            $('#linkHeading').show();
                            var condetailsDiv = $('#txtConDesc,#Connectiondetails,#workflowdetails,#condetails');
                            condetailsDiv.hide();

                        }
                        else {
                            $('#nodedetails,#condetails').hide();
                            $('#workflowdetails').show();
                            $('#linkHeading').hide();
                            var condetailsDiv = $('#txtConDesc,#Connectiondetails');
                            condetailsDiv.hide();
                        }


                    }
                    else if (typeof data["fromPort"] != 'undefined' && data["from"] == data["to"]) {
                        var condetailsDiv = $('#txtConDesc,#Connectiondetails,#condetails');
                        condetailsDiv.show();
                    }
                    else {
                        $("#condetails").fadeOut(500, function () {
                            $("#linkHeading").fadeIn(500, function () {
                            });
                        });
                        var condetailsDiv = $('#txtConDesc,#Connectiondetails');
                        condetailsDiv.hide();
                    }


                }
                else {
                    $("#linkHeading").fadeOut(500, function () {
                        $("#condetails").fadeIn(500, function () {
                        });
                    });

                    $('#txtConDesc').show();
                }
            }
        }
        else {
            var actionSpan = $('#actions').css('display', 'none');
            var divs = $('#condetails,#nodedetails,#txtConDesc,#Connectiondetails,#linkHeading,#touchpointdetails,#getdetails,#processdetails');
            divs.hide();
            var workflowdiv = $('#workflowdetails,#noSelectiondetails');
            workflowdiv.show();
        }
        //$('.toggler-close').trigger('click');
    };
    this.showIfExists = function (inspector, property) {
        var data = (inspector instanceof go.Part) ? inspector.data : inspector;
        if (typeof data[property] !== 'undefined')
            return true;
        return false;
    };

    inspector = new Inspector('nodedetails', this.showInspectorHeader, this.graph,
      {

          properties: {

              "key": { readOnly: true, show: false },
              "fill": { show: false, type: 'color' },
              "background": { show: false, type: 'color' },
              "color": { show: false, type: 'color' },
              "stroke": { show: false, type: 'color' },
              "text": { show: false, store: obj.TextStoreList, linkStore: obj.LinkTextStoreList },
              "text": { show: this.showIfExists, readOnly: true },
              "from": { show: false, readOnly: true },
              "to": { show: false, readOnly: true },
              "fromPort": { show: false, readOnly: true },
              "toPort": { show: false, readOnly: true },
              "loc": { show: false, readOnly: true },
              "figure": { show: false, store: obj.shapeList },
              "touchPointId": { show: false },
              "connectionId": { show: false },
              "eventId": { show: false },
              "category": { show: false },
              "group": { show: false },
              "type": { show: false },
              "points": { show: false },
              "angle": { show: false },
              "isGroup": { show: false },
              "Entity": { show: this.showIfExists, readOnly: false },
              "Xpath": { show: this.showIfExists, readOnly: false },
              "BatchSize": { show: this.showIfExists, readOnly: false, type: 'number', pattern: '[0-9]', isInteger: true, min: '1', max: '99', altname: 'Batch Size' },
              "geo":{show:false},
              "Delaytime": { show: true, type: 'number', pattern: '[0-9]', isInteger: true, min: '1', altname: 'Delay Time' },
              "width": { show: false },
              "ExecutionTime": { show: this.showIfExists, type: 'number', pattern: '[0-9]', isInteger: true, min: '1', max: '99', altname: 'Execution Time' },
              "isDirty": { show: false },
              "isSubGraphExpanded": { show: false },
              "ExecutionOrder": { show: this.showIfExists, readOnly: false, type: 'number', pattern: '[0-9]', isInteger: true, min: '1', max: '99', altname: 'Execution Order' },
              "ExecutionChk": { show: false }
          }
      });

    function checkLoops(fromnode, fromport, tonode, toport) {
        var links = model.linkDataArray;
        if (fromnode.key == tonode.key)
            return true;
        var nodeInTraversal = [fromnode.to]

        return true;
    };
    //this.graph.validCycle = go.Diagram.CycleDestinationTree;
    //this.graph.validCycle = go.Diagram.CycleDestinationTree;
    return this;
};