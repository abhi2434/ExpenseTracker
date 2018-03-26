var Graph = function (obj) {
    this.currentGroup;
    this.diagram = go.GraphObject.make;
    var self = this;
    this.graph = this.diagram(go.Diagram, obj.elementId,
    {
        initialContentAlignment: go.Spot.Center,
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
        self.deleteSegment(e, obj, true);
    };
    this.deleteSegment = function (e, obj, showConfirm) {
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

    this.graph.addDiagramListener("SelectionDeleting", function (e, obj) {
        var node = e.subject.first();
        var data = node.data;
        if (data.text) {
            if (!confirm("Are you sure to delete " + data.text + " from the workflow?"))
                e.cancel = true;
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
                      this.diagram(go.Shape, "Circle",
                       { strokeWidth: 4, width: 60, height: 60 },
                          new go.Binding("fill", "background"),
                          new go.Binding("stroke", "stroke")),
                      this.diagram(go.Shape,
                      new go.Binding("geometryString", "geometryString")),
                     {
                         toolTip:
                         this.diagram(go.Adornment, "Auto",
                         this.diagram(go.Shape, { fill: "LightYellow", stroke: "#888", strokeWidth: 2 }),
                         this.diagram(go.TextBlock, { margin: 8, stroke: "#888", font: "bold 16px sans-serif" },
                        new go.Binding("text", "text"),
                        new go.Binding("stroke", "color"),
                        new go.Binding("touchPointId", "tpId")))
                     },
                          this.diagram("Button",
                                {
                                    alignment: go.Spot.TopRight,
                                    "_buttonFillNormal": "#000000",
                                    "_buttonStrokeNormal": "#000000",
                                    "_buttonFillOver": "#000000",
                                    "_buttonStrokeOver": "#000000"
                                },
                this.diagram(go.Shape, "XLine", { width: 5, height: 5, stroke: "#ff0000" }),
                       { click: this.deleteSegmentwithConfirm }),
                        makePort(this, "T", go.Spot.Top, false, true),
                        makePort(this, "B", go.Spot.Bottom, true, false)
                  ));


        this.graph.nodeTemplateMap.add("Start",
                  this.diagram(go.Node, "Auto", nodeStyle,
                            new go.Binding("location", "loc", go.Point.parse),
                      this.diagram(go.Shape, "Circle",
                       { strokeWidth: 4, width: 60, height: 60 },
                          new go.Binding("fill", "background"),
                          new go.Binding("stroke", "stroke")),
                      this.diagram(go.Shape,
                      new go.Binding("geometryString", "geometryString")),
                     {
                         toolTip:
                         this.diagram(go.Adornment, "Auto",
                         this.diagram(go.Shape, { fill: "LightYellow", stroke: "#888", strokeWidth: 2 }),
                         this.diagram(go.TextBlock, { margin: 8, stroke: "#888", font: "bold 16px sans-serif" },
                        new go.Binding("text", "text"),
                        new go.Binding("stroke", "color"),
                        new go.Binding("touchPointId", "tpId")))
                     },

                this.diagram("Button",
                {
                    alignment: go.Spot.TopRight,
                    "_buttonFillNormal": "#000000",
                    "_buttonStrokeNormal": "#FFFFFF",
                    "_buttonFillOver": "#000000",
                    "_buttonStrokeOver": "#FFFFFF"
                },
                this.diagram(go.Shape, "XLine", { width: 5, height: 5, stroke: "#ff0000" }),
                    { click: this.deleteSegmentwithConfirm }),
                    makePort(this, "B", go.Spot.Bottom, true, false)
                  ));

        this.graph.nodeTemplateMap.add("End",
                    this.diagram(go.Node, "Auto", nodeStyle,
                            new go.Binding("location", "loc", go.Point.parse),
                      this.diagram(go.Shape, "Circle",
                       { strokeWidth: 4, width: 60, height: 60 },
                          new go.Binding("fill", "background"),
                          new go.Binding("stroke", "stroke")),
                      this.diagram(go.Shape,
                      new go.Binding("geometryString", "geometryString")),
                     {
                         toolTip:
                         this.diagram(go.Adornment, "Auto",
                         this.diagram(go.Shape, { fill: "LightYellow", stroke: "#888", strokeWidth: 2 }),
                         this.diagram(go.TextBlock, { margin: 8, stroke: "#888", font: "bold 16px sans-serif" },
                        new go.Binding("text", "text"),
                        new go.Binding("stroke", "color"),
                        new go.Binding("touchPointId", "tpId")))
                     },

                         this.diagram("Button",
                        {
                            alignment: go.Spot.TopRight,
                            "_buttonFillNormal": "#000000",
                            "_buttonStrokeNormal": "#FFFFFF",
                            "_buttonFillOver": "#000000",
                            "_buttonStrokeOver": "#FFFFFF"
                        },
                this.diagram(go.Shape, "XLine", { width: 5, height: 5, stroke: "#ff0000" }),
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
                    fill: "rgba(128,128,128,0.33)"
                }
        ), this.diagram("Button",
                {
                    alignment: go.Spot.TopRight,
                    "_buttonFillNormal": "#000000",
                    "_buttonStrokeNormal": "#FFFFFF",
                    "_buttonFillOver": "#000000",
                    "_buttonStrokeOver": "#FFFFFF"
                },
                this.diagram(go.Shape, "XLine", { width: 5, height: 5, stroke: "#ff0000" }),
                    { click: this.deleteSegmentwithConfirm }),
            makePort(this, "T", go.Spot.Top, false, true),
        this.diagram(go.Panel, "Vertical",  // position header above the subgraph
            { defaultAlignment: go.Spot.TopLeft },
        this.diagram(go.Panel, "Horizontal",  // the header
          { defaultAlignment: go.Spot.Top },
          this.diagram(go.TextBlock,     // group title near top, next to button
            { font: "Bold 12pt Sans-Serif", alignment: go.Spot.Left, },
            new go.Binding("text", "text")),
          this.diagram("SubGraphExpanderButton", { margin: new go.Margin(3, 3, 5, 5), alignment: go.Spot.Right })  // this Panel acts as a Button
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

    var templateDiagram = this.diagram(go.Node, "Auto",
        this.diagram(go.Panel, "Auto",
          this.diagram(go.Panel, "Auto",
                this.diagram(go.Panel, "Auto",
                this.diagram(go.Shape, "Rectangle", { margin: new go.Margin(0, 0, 0, 30), width: 200, height: 60 },
                    new go.Binding("fill", "background")),
                this.diagram(go.TextBlock, { margin: 8, font: "Italic 12px sans-serif" },
                    new go.Binding("text", "text"))),
                    this.diagram(go.Shape, "Circle",
                            { width: 60, height: 60, alignment: go.Spot.Left },
                    new go.Binding("fill", "background"),
                    new go.Binding("stroke", "stroke")),
            this.diagram(go.Shape,
                { margin: new go.Margin(3, 3, 3, 20), strokeWidth: 1, alignment: go.Spot.Left },
                new go.Binding("geometryString", "geometryString"), new go.Binding("fill", "color")))
                ),
           {
               toolTip:
               this.diagram(go.Adornment, "Auto",
               this.diagram(go.Shape, { fill: "LightYellow", stroke: "#888", strokeWidth: 2 }),
               this.diagram(go.TextBlock, { margin: 8, stroke: "#888", font: "bold 16px sans-serif" },
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
    this.addPaletteDatas = function (datas) {

        for (var i = 0; i < datas.length; i++)
            this.addPaletteData(datas[i]);
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
        this.setModelData(data, 7, "type");
        this.setModelData(data, "M11.366 22.564l1.291-1.807-1.414-1.414-1.807 1.291c-0.335-0.187-0.694-0.337-1.071-0.444l-0.365-2.19h-2l-0.365 2.19c-0.377 0.107-0.736 0.256-1.071 0.444l-1.807-1.291-1.414 1.414 1.291 1.807c-0.187 0.335-0.337 0.694-0.443 1.071l-2.19 0.365v2l2.19 0.365c0.107 0.377 0.256 0.736 0.444 1.071l-1.291 1.807 1.414 1.414 1.807-1.291c0.335 0.187 0.694 0.337 1.071 0.444l0.365 2.19h2l0.365-2.19c0.377-0.107 0.736-0.256 1.071-0.444l1.807 1.291 1.414-1.414-1.291-1.807c0.187-0.335 0.337-0.694 0.444-1.071l2.19-0.365v-2l-2.19-0.365c-0.107-0.377-0.256-0.736-0.444-1.071zM7 27c-1.105 0-2-0.895-2-2s0.895-2 2-2 2 0.895 2 2-0.895 2-2 2zM32 12v-2l-2.106-0.383c-0.039-0.251-0.088-0.499-0.148-0.743l1.799-1.159-0.765-1.848-2.092 0.452c-0.132-0.216-0.273-0.426-0.422-0.629l1.219-1.761-1.414-1.414-1.761 1.219c-0.203-0.149-0.413-0.29-0.629-0.422l0.452-2.092-1.848-0.765-1.159 1.799c-0.244-0.059-0.492-0.109-0.743-0.148l-0.383-2.106h-2l-0.383 2.106c-0.251 0.039-0.499 0.088-0.743 0.148l-1.159-1.799-1.848 0.765 0.452 2.092c-0.216 0.132-0.426 0.273-0.629 0.422l-1.761-1.219-1.414 1.414 1.219 1.761c-0.149 0.203-0.29 0.413-0.422 0.629l-2.092-0.452-0.765 1.848 1.799 1.159c-0.059 0.244-0.109 0.492-0.148 0.743l-2.106 0.383v2l2.106 0.383c0.039 0.251 0.088 0.499 0.148 0.743l-1.799 1.159 0.765 1.848 2.092-0.452c0.132 0.216 0.273 0.426 0.422 0.629l-1.219 1.761 1.414 1.414 1.761-1.219c0.203 0.149 0.413 0.29 0.629 0.422l-0.452 2.092 1.848 0.765 1.159-1.799c0.244 0.059 0.492 0.109 0.743 0.148l0.383 2.106h2l0.383-2.106c0.251-0.039 0.499-0.088 0.743-0.148l1.159 1.799 1.848-0.765-0.452-2.092c0.216-0.132 0.426-0.273 0.629-0.422l1.761 1.219 1.414-1.414-1.219-1.761c0.149-0.203 0.29-0.413 0.422-0.629l2.092 0.452 0.765-1.848-1.799-1.159c0.059-0.244 0.109-0.492 0.148-0.743l2.106-0.383zM21 15.35c-2.402 0-4.35-1.948-4.35-4.35s1.948-4.35 4.35-4.35 4.35 1.948 4.35 4.35c0 2.402-1.948 4.35-4.35 4.35z", "geometryString");
        this.setModelData(data, "#888", "stroke");
        this.setModelData(data, "#E87E04", "background");
        this.setModelData(data, "#364150", "color");
        this.setModelData(data, key1, "group");
        this.setModelData(data, "Default", "category");


        this.addDatas([this.getPost(id, key2, key1, cid), this.getGroup(id, key1, val, cid)]);
        this.addLinks([{ key: generateUUID(), from: key, to: key2 }]);
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
        model.setDataProperty(data, "geometryString", "M13.922 5.626c-0.051-2.011-1.695-3.626-3.717-3.626-1.184 0-2.239 0.555-2.92 1.418-0.382-0.494-0.98-0.812-1.652-0.812-1.153 0-2.088 0.936-2.088 2.089 0 0.101 0.007 0.199 0.021 0.296-0.175-0.032-0.356-0.049-0.54-0.049-1.672-0-3.027 1.356-3.027 3.029s1.355 3.029 3.027 3.029h1.434l3.539 3.664 3.539-3.664 1.742-0c1.502-0.003 2.719-1.222 2.719-2.726 0-1.283-0.886-2.358-2.078-2.648zM8 13l-3-3h2v-3h2v3h2l-3 3z");
        model.commitTransaction("changegeo");

        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#F7CA18");
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

        this.addDatas([{
            key: key2,
            text: "Merger",
            stroke: "#888",
            background: "#3598DC",
            color: "#364150",
            category: "Default",
            touchPointId: id,
            connectionId: cid,
            geometryString: "M32 8l-8-8v6c-4.087 0-7.211 0.975-9.552 2.982-0.164 0.141-0.321 0.284-0.474 0.43 0.86 1.193 1.522 2.422 2.118 3.597 1.51-1.825 3.689-3.008 7.907-3.008v12c-6.764 0-8.285-3.043-10.211-6.894-1.072-2.144-2.181-4.361-4.237-6.124-2.341-2.006-5.465-2.982-9.552-2.982v4c6.764 0 8.285 3.043 10.211 6.894 1.072 2.144 2.181 4.361 4.237 6.124 2.341 2.006 5.465 2.982 9.552 2.982v6l8-8-8-8 8-8zM0 22v4c4.087 0 7.211-0.975 9.552-2.982 0.164-0.141 0.321-0.284 0.474-0.43-0.86-1.193-1.522-2.422-2.118-3.597-1.51 1.825-3.689 3.009-7.907 3.009z",
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
            background: "#E87E04",
            color: "#364150",
            category: "Default",
            geometryString: "M11.366 22.564l1.291-1.807-1.414-1.414-1.807 1.291c-0.335-0.187-0.694-0.337-1.071-0.444l-0.365-2.19h-2l-0.365 2.19c-0.377 0.107-0.736 0.256-1.071 0.444l-1.807-1.291-1.414 1.414 1.291 1.807c-0.187 0.335-0.337 0.694-0.443 1.071l-2.19 0.365v2l2.19 0.365c0.107 0.377 0.256 0.736 0.444 1.071l-1.291 1.807 1.414 1.414 1.807-1.291c0.335 0.187 0.694 0.337 1.071 0.444l0.365 2.19h2l0.365-2.19c0.377-0.107 0.736-0.256 1.071-0.444l1.807 1.291 1.414-1.414-1.291-1.807c0.187-0.335 0.337-0.694 0.444-1.071l2.19-0.365v-2l-2.19-0.365c-0.107-0.377-0.256-0.736-0.444-1.071zM7 27c-1.105 0-2-0.895-2-2s0.895-2 2-2 2 0.895 2 2-0.895 2-2 2zM32 12v-2l-2.106-0.383c-0.039-0.251-0.088-0.499-0.148-0.743l1.799-1.159-0.765-1.848-2.092 0.452c-0.132-0.216-0.273-0.426-0.422-0.629l1.219-1.761-1.414-1.414-1.761 1.219c-0.203-0.149-0.413-0.29-0.629-0.422l0.452-2.092-1.848-0.765-1.159 1.799c-0.244-0.059-0.492-0.109-0.743-0.148l-0.383-2.106h-2l-0.383 2.106c-0.251 0.039-0.499 0.088-0.743 0.148l-1.159-1.799-1.848 0.765 0.452 2.092c-0.216 0.132-0.426 0.273-0.629 0.422l-1.761-1.219-1.414 1.414 1.219 1.761c-0.149 0.203-0.29 0.413-0.422 0.629l-2.092-0.452-0.765 1.848 1.799 1.159c-0.059 0.244-0.109 0.492-0.148 0.743l-2.106 0.383v2l2.106 0.383c0.039 0.251 0.088 0.499 0.148 0.743l-1.799 1.159 0.765 1.848 2.092-0.452c0.132 0.216 0.273 0.426 0.422 0.629l-1.219 1.761 1.414 1.414 1.761-1.219c0.203 0.149 0.413 0.29 0.629 0.422l-0.452 2.092 1.848 0.765 1.159-1.799c0.244 0.059 0.492 0.109 0.743 0.148l0.383 2.106h2l0.383-2.106c0.251-0.039 0.499-0.088 0.743-0.148l1.159 1.799 1.848-0.765-0.452-2.092c0.216-0.132 0.426-0.273 0.629-0.422l1.761 1.219 1.414-1.414-1.219-1.761c0.149-0.203 0.29-0.413 0.422-0.629l2.092 0.452 0.765-1.848-1.799-1.159c0.059-0.244 0.109-0.492 0.148-0.743l2.106-0.383zM21 15.35c-2.402 0-4.35-1.948-4.35-4.35s1.948-4.35 4.35-4.35 4.35 1.948 4.35 4.35c0 2.402-1.948 4.35-4.35 4.35z",
            touchPointId: id,
            connectionId: cid,
            group: key1,
            type: 7,
            //loc: "450 0"

        }, {
            key: key4,
            text: "Post",
            stroke: "#888",
            background: "#F7CA18",
            color: "#364150",
            category: "Default",
            geometryString: "M13.942 6.039c0.038-0.174 0.058-0.354 0.058-0.539 0-1.381-1.119-2.5-2.5-2.5-0.222 0-0.438 0.029-0.643 0.084-0.387-1.209-1.52-2.084-2.857-2.084-1.365 0-2.516 0.911-2.88 2.159-0.355-0.103-0.731-0.159-1.12-0.159-2.209 0-4 1.791-4 4s1.791 4 4 4h2v3h4v-3h3.5c1.381 0 2.5-1.119 2.5-2.5 0-1.23-0.888-2.253-2.058-2.461zM9 10v3h-2v-3h-2.5l3.5-3.5 3.5 3.5h-2.5z",
            touchPointId: id,
            connectionId: cid,
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
        this.addLinks([{ key: generateUUID(), from: key, to: key2 }, { key: generateUUID(), from: key2, to: key3 }, { key: generateUUID(), from: key3, to: key4 }]);


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
        model.setDataProperty(data, "geometryString", "M13.922 5.626c-0.051-2.011-1.695-3.626-3.717-3.626-1.184 0-2.239 0.555-2.92 1.418-0.382-0.494-0.98-0.812-1.652-0.812-1.153 0-2.088 0.936-2.088 2.089 0 0.101 0.007 0.199 0.021 0.296-0.175-0.032-0.356-0.049-0.54-0.049-1.672-0-3.027 1.356-3.027 3.029s1.355 3.029 3.027 3.029h1.434l3.539 3.664 3.539-3.664 1.742-0c1.502-0.003 2.719-1.222 2.719-2.726 0-1.283-0.886-2.358-2.078-2.648zM8 13l-3-3h2v-3h2v3h2l-3 3z");
        model.commitTransaction("changegeo");

        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#F7CA18");
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


        this.addDatas([{
            key: key2,
            text: "Process",
            stroke: "#888",
            background: "#E87E04",
            color: "#364150",
            category: "Default",
            geometryString: "M11.366 22.564l1.291-1.807-1.414-1.414-1.807 1.291c-0.335-0.187-0.694-0.337-1.071-0.444l-0.365-2.19h-2l-0.365 2.19c-0.377 0.107-0.736 0.256-1.071 0.444l-1.807-1.291-1.414 1.414 1.291 1.807c-0.187 0.335-0.337 0.694-0.443 1.071l-2.19 0.365v2l2.19 0.365c0.107 0.377 0.256 0.736 0.444 1.071l-1.291 1.807 1.414 1.414 1.807-1.291c0.335 0.187 0.694 0.337 1.071 0.444l0.365 2.19h2l0.365-2.19c0.377-0.107 0.736-0.256 1.071-0.444l1.807 1.291 1.414-1.414-1.291-1.807c0.187-0.335 0.337-0.694 0.444-1.071l2.19-0.365v-2l-2.19-0.365c-0.107-0.377-0.256-0.736-0.444-1.071zM7 27c-1.105 0-2-0.895-2-2s0.895-2 2-2 2 0.895 2 2-0.895 2-2 2zM32 12v-2l-2.106-0.383c-0.039-0.251-0.088-0.499-0.148-0.743l1.799-1.159-0.765-1.848-2.092 0.452c-0.132-0.216-0.273-0.426-0.422-0.629l1.219-1.761-1.414-1.414-1.761 1.219c-0.203-0.149-0.413-0.29-0.629-0.422l0.452-2.092-1.848-0.765-1.159 1.799c-0.244-0.059-0.492-0.109-0.743-0.148l-0.383-2.106h-2l-0.383 2.106c-0.251 0.039-0.499 0.088-0.743 0.148l-1.159-1.799-1.848 0.765 0.452 2.092c-0.216 0.132-0.426 0.273-0.629 0.422l-1.761-1.219-1.414 1.414 1.219 1.761c-0.149 0.203-0.29 0.413-0.422 0.629l-2.092-0.452-0.765 1.848 1.799 1.159c-0.059 0.244-0.109 0.492-0.148 0.743l-2.106 0.383v2l2.106 0.383c0.039 0.251 0.088 0.499 0.148 0.743l-1.799 1.159 0.765 1.848 2.092-0.452c0.132 0.216 0.273 0.426 0.422 0.629l-1.219 1.761 1.414 1.414 1.761-1.219c0.203 0.149 0.413 0.29 0.629 0.422l-0.452 2.092 1.848 0.765 1.159-1.799c0.244 0.059 0.492 0.109 0.743 0.148l0.383 2.106h2l0.383-2.106c0.251-0.039 0.499-0.088 0.743-0.148l1.159 1.799 1.848-0.765-0.452-2.092c0.216-0.132 0.426-0.273 0.629-0.422l1.761 1.219 1.414-1.414-1.219-1.761c0.149-0.203 0.29-0.413 0.422-0.629l2.092 0.452 0.765-1.848-1.799-1.159c0.059-0.244 0.109-0.492 0.148-0.743l2.106-0.383zM21 15.35c-2.402 0-4.35-1.948-4.35-4.35s1.948-4.35 4.35-4.35 4.35 1.948 4.35 4.35c0 2.402-1.948 4.35-4.35 4.35z",
            touchPointId: id,
            connectionId: cid,
            group: key1,
            type: 7,
            //loc: "250 0"

        }, {
            key: key3,
            text: "Merger",
            stroke: "#888",
            background: "#3598DC",
            color: "#364150",
            category: "Default",
            touchPointId: id,
            connectionId: cid,
            geometryString: "M32 8l-8-8v6c-4.087 0-7.211 0.975-9.552 2.982-0.164 0.141-0.321 0.284-0.474 0.43 0.86 1.193 1.522 2.422 2.118 3.597 1.51-1.825 3.689-3.008 7.907-3.008v12c-6.764 0-8.285-3.043-10.211-6.894-1.072-2.144-2.181-4.361-4.237-6.124-2.341-2.006-5.465-2.982-9.552-2.982v4c6.764 0 8.285 3.043 10.211 6.894 1.072 2.144 2.181 4.361 4.237 6.124 2.341 2.006 5.465 2.982 9.552 2.982v6l8-8-8-8 8-8zM0 22v4c4.087 0 7.211-0.975 9.552-2.982 0.164-0.141 0.321-0.284 0.474-0.43-0.86-1.193-1.522-2.422-2.118-3.597-1.51 1.825-3.689 3.009-7.907 3.009z",
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
            background: "#F7CA18",
            color: "#364150",
            category: "Default",
            geometryString: "M13.942 6.039c0.038-0.174 0.058-0.354 0.058-0.539 0-1.381-1.119-2.5-2.5-2.5-0.222 0-0.438 0.029-0.643 0.084-0.387-1.209-1.52-2.084-2.857-2.084-1.365 0-2.516 0.911-2.88 2.159-0.355-0.103-0.731-0.159-1.12-0.159-2.209 0-4 1.791-4 4s1.791 4 4 4h2v3h4v-3h3.5c1.381 0 2.5-1.119 2.5-2.5 0-1.23-0.888-2.253-2.058-2.461zM9 10v3h-2v-3h-2.5l3.5-3.5 3.5 3.5h-2.5z",
            touchPointId: id,
            connectionId: cid,
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
        this.addLinks([{ key: generateUUID(), from: key, to: key2 }, { key: generateUUID(), from: key2, to: key3 }, { key: generateUUID(), from: key3, to: key4 }]);

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
        model.setDataProperty(data, "geometryString", "M13.922 5.626c-0.051-2.011-1.695-3.626-3.717-3.626-1.184 0-2.239 0.555-2.92 1.418-0.382-0.494-0.98-0.812-1.652-0.812-1.153 0-2.088 0.936-2.088 2.089 0 0.101 0.007 0.199 0.021 0.296-0.175-0.032-0.356-0.049-0.54-0.049-1.672-0-3.027 1.356-3.027 3.029s1.355 3.029 3.027 3.029h1.434l3.539 3.664 3.539-3.664 1.742-0c1.502-0.003 2.719-1.222 2.719-2.726 0-1.283-0.886-2.358-2.078-2.648zM8 13l-3-3h2v-3h2v3h2l-3 3z");
        model.commitTransaction("changegeo");


        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#F7CA18");
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

        this.addDatas([{
            key: key2,
            text: "Split",
            stroke: "#888",
            background: "#36c6d3",
            color: "#364150",
            category: "Default",
            geometryString: "M16 0h-6.5l2.5 2.5-3 3 1.5 1.5 3-3 2.5 2.5z x M16 16v-6.5l-2.5 2.5-3-3-1.5 1.5 3 3-2.5 2.5z x M0 16h6.5l-2.5-2.5 3-3-1.5-1.5-3 3-2.5-2.5z x M0 0v6.5l2.5-2.5 3 3 1.5-1.5-3-3 2.5-2.5z",
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
            background: "#E87E04",
            color: "#364150",
            category: "Default",
            geometryString: "M11.366 22.564l1.291-1.807-1.414-1.414-1.807 1.291c-0.335-0.187-0.694-0.337-1.071-0.444l-0.365-2.19h-2l-0.365 2.19c-0.377 0.107-0.736 0.256-1.071 0.444l-1.807-1.291-1.414 1.414 1.291 1.807c-0.187 0.335-0.337 0.694-0.443 1.071l-2.19 0.365v2l2.19 0.365c0.107 0.377 0.256 0.736 0.444 1.071l-1.291 1.807 1.414 1.414 1.807-1.291c0.335 0.187 0.694 0.337 1.071 0.444l0.365 2.19h2l0.365-2.19c0.377-0.107 0.736-0.256 1.071-0.444l1.807 1.291 1.414-1.414-1.291-1.807c0.187-0.335 0.337-0.694 0.444-1.071l2.19-0.365v-2l-2.19-0.365c-0.107-0.377-0.256-0.736-0.444-1.071zM7 27c-1.105 0-2-0.895-2-2s0.895-2 2-2 2 0.895 2 2-0.895 2-2 2zM32 12v-2l-2.106-0.383c-0.039-0.251-0.088-0.499-0.148-0.743l1.799-1.159-0.765-1.848-2.092 0.452c-0.132-0.216-0.273-0.426-0.422-0.629l1.219-1.761-1.414-1.414-1.761 1.219c-0.203-0.149-0.413-0.29-0.629-0.422l0.452-2.092-1.848-0.765-1.159 1.799c-0.244-0.059-0.492-0.109-0.743-0.148l-0.383-2.106h-2l-0.383 2.106c-0.251 0.039-0.499 0.088-0.743 0.148l-1.159-1.799-1.848 0.765 0.452 2.092c-0.216 0.132-0.426 0.273-0.629 0.422l-1.761-1.219-1.414 1.414 1.219 1.761c-0.149 0.203-0.29 0.413-0.422 0.629l-2.092-0.452-0.765 1.848 1.799 1.159c-0.059 0.244-0.109 0.492-0.148 0.743l-2.106 0.383v2l2.106 0.383c0.039 0.251 0.088 0.499 0.148 0.743l-1.799 1.159 0.765 1.848 2.092-0.452c0.132 0.216 0.273 0.426 0.422 0.629l-1.219 1.761 1.414 1.414 1.761-1.219c0.203 0.149 0.413 0.29 0.629 0.422l-0.452 2.092 1.848 0.765 1.159-1.799c0.244 0.059 0.492 0.109 0.743 0.148l0.383 2.106h2l0.383-2.106c0.251-0.039 0.499-0.088 0.743-0.148l1.159 1.799 1.848-0.765-0.452-2.092c0.216-0.132 0.426-0.273 0.629-0.422l1.761 1.219 1.414-1.414-1.219-1.761c0.149-0.203 0.29-0.413 0.422-0.629l2.092 0.452 0.765-1.848-1.799-1.159c0.059-0.244 0.109-0.492 0.148-0.743l2.106-0.383zM21 15.35c-2.402 0-4.35-1.948-4.35-4.35s1.948-4.35 4.35-4.35 4.35 1.948 4.35 4.35c0 2.402-1.948 4.35-4.35 4.35z",
            touchPointId: id,
            connectionId: cid,
            group: key1,
            type: 7,
            //loc: "450 0"
        }, {
            key: key4,
            text: "Post",
            stroke: "#888",
            background: "#F7CA18",
            color: "#364150",
            category: "Default",
            geometryString: "M13.942 6.039c0.038-0.174 0.058-0.354 0.058-0.539 0-1.381-1.119-2.5-2.5-2.5-0.222 0-0.438 0.029-0.643 0.084-0.387-1.209-1.52-2.084-2.857-2.084-1.365 0-2.516 0.911-2.88 2.159-0.355-0.103-0.731-0.159-1.12-0.159-2.209 0-4 1.791-4 4s1.791 4 4 4h2v3h4v-3h3.5c1.381 0 2.5-1.119 2.5-2.5 0-1.23-0.888-2.253-2.058-2.461zM9 10v3h-2v-3h-2.5l3.5-3.5 3.5 3.5h-2.5z",
            touchPointId: id,
            connectionId: cid,
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
        this.addLinks([{ key: generateUUID(), from: key, to: key2 }, { key: generateUUID(), from: key2, to: key3 }, { key: generateUUID(), from: key3, to: key4 }]);

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
        model.setDataProperty(data, "geometryString", "M13.922 5.626c-0.051-2.011-1.695-3.626-3.717-3.626-1.184 0-2.239 0.555-2.92 1.418-0.382-0.494-0.98-0.812-1.652-0.812-1.153 0-2.088 0.936-2.088 2.089 0 0.101 0.007 0.199 0.021 0.296-0.175-0.032-0.356-0.049-0.54-0.049-1.672-0-3.027 1.356-3.027 3.029s1.355 3.029 3.027 3.029h1.434l3.539 3.664 3.539-3.664 1.742-0c1.502-0.003 2.719-1.222 2.719-2.726 0-1.283-0.886-2.358-2.078-2.648zM8 13l-3-3h2v-3h2v3h2l-3 3z");
        model.commitTransaction("changegeo");

        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#F7CA18");
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

        this.addDatas([{
            key: key2,
            text: "Process",
            stroke: "#888",
            background: "#E87E04",
            color: "#364150",
            category: "Default",
            geometryString: "M11.366 22.564l1.291-1.807-1.414-1.414-1.807 1.291c-0.335-0.187-0.694-0.337-1.071-0.444l-0.365-2.19h-2l-0.365 2.19c-0.377 0.107-0.736 0.256-1.071 0.444l-1.807-1.291-1.414 1.414 1.291 1.807c-0.187 0.335-0.337 0.694-0.443 1.071l-2.19 0.365v2l2.19 0.365c0.107 0.377 0.256 0.736 0.444 1.071l-1.291 1.807 1.414 1.414 1.807-1.291c0.335 0.187 0.694 0.337 1.071 0.444l0.365 2.19h2l0.365-2.19c0.377-0.107 0.736-0.256 1.071-0.444l1.807 1.291 1.414-1.414-1.291-1.807c0.187-0.335 0.337-0.694 0.444-1.071l2.19-0.365v-2l-2.19-0.365c-0.107-0.377-0.256-0.736-0.444-1.071zM7 27c-1.105 0-2-0.895-2-2s0.895-2 2-2 2 0.895 2 2-0.895 2-2 2zM32 12v-2l-2.106-0.383c-0.039-0.251-0.088-0.499-0.148-0.743l1.799-1.159-0.765-1.848-2.092 0.452c-0.132-0.216-0.273-0.426-0.422-0.629l1.219-1.761-1.414-1.414-1.761 1.219c-0.203-0.149-0.413-0.29-0.629-0.422l0.452-2.092-1.848-0.765-1.159 1.799c-0.244-0.059-0.492-0.109-0.743-0.148l-0.383-2.106h-2l-0.383 2.106c-0.251 0.039-0.499 0.088-0.743 0.148l-1.159-1.799-1.848 0.765 0.452 2.092c-0.216 0.132-0.426 0.273-0.629 0.422l-1.761-1.219-1.414 1.414 1.219 1.761c-0.149 0.203-0.29 0.413-0.422 0.629l-2.092-0.452-0.765 1.848 1.799 1.159c-0.059 0.244-0.109 0.492-0.148 0.743l-2.106 0.383v2l2.106 0.383c0.039 0.251 0.088 0.499 0.148 0.743l-1.799 1.159 0.765 1.848 2.092-0.452c0.132 0.216 0.273 0.426 0.422 0.629l-1.219 1.761 1.414 1.414 1.761-1.219c0.203 0.149 0.413 0.29 0.629 0.422l-0.452 2.092 1.848 0.765 1.159-1.799c0.244 0.059 0.492 0.109 0.743 0.148l0.383 2.106h2l0.383-2.106c0.251-0.039 0.499-0.088 0.743-0.148l1.159 1.799 1.848-0.765-0.452-2.092c0.216-0.132 0.426-0.273 0.629-0.422l1.761 1.219 1.414-1.414-1.219-1.761c0.149-0.203 0.29-0.413 0.422-0.629l2.092 0.452 0.765-1.848-1.799-1.159c0.059-0.244 0.109-0.492 0.148-0.743l2.106-0.383zM21 15.35c-2.402 0-4.35-1.948-4.35-4.35s1.948-4.35 4.35-4.35 4.35 1.948 4.35 4.35c0 2.402-1.948 4.35-4.35 4.35z",
            touchPointId: id,
            connectionId: cid,
            group: key1,
            type: 7,
            //loc: "250 0"
        }, {
            key: key3,
            text: "Split",
            stroke: "#888",
            background: "#36c6d3",
            color: "#364150",
            category: "Default",
            geometryString: "M16 0h-6.5l2.5 2.5-3 3 1.5 1.5 3-3 2.5 2.5z x M16 16v-6.5l-2.5 2.5-3-3-1.5 1.5 3 3-2.5 2.5z x M0 16h6.5l-2.5-2.5 3-3-1.5-1.5-3 3-2.5-2.5z x M0 0v6.5l2.5-2.5 3 3 1.5-1.5-3-3 2.5-2.5z",
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
            background: "#F7CA18",
            color: "#364150",
            category: "Default",
            geometryString: "M13.942 6.039c0.038-0.174 0.058-0.354 0.058-0.539 0-1.381-1.119-2.5-2.5-2.5-0.222 0-0.438 0.029-0.643 0.084-0.387-1.209-1.52-2.084-2.857-2.084-1.365 0-2.516 0.911-2.88 2.159-0.355-0.103-0.731-0.159-1.12-0.159-2.209 0-4 1.791-4 4s1.791 4 4 4h2v3h4v-3h3.5c1.381 0 2.5-1.119 2.5-2.5 0-1.23-0.888-2.253-2.058-2.461zM9 10v3h-2v-3h-2.5l3.5-3.5 3.5 3.5h-2.5z",
            touchPointId: id,
            connectionId: cid,
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

        this.addLinks([{ key: generateUUID(), from: key, to: key2 }, { key: generateUUID(), from: key2, to: key3 }, { key: generateUUID(), from: key3, to: key4 }]);

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
        model.setDataProperty(data, "geometryString", "M13.922 5.626c-0.051-2.011-1.695-3.626-3.717-3.626-1.184 0-2.239 0.555-2.92 1.418-0.382-0.494-0.98-0.812-1.652-0.812-1.153 0-2.088 0.936-2.088 2.089 0 0.101 0.007 0.199 0.021 0.296-0.175-0.032-0.356-0.049-0.54-0.049-1.672-0-3.027 1.356-3.027 3.029s1.355 3.029 3.027 3.029h1.434l3.539 3.664 3.539-3.664 1.742-0c1.502-0.003 2.719-1.222 2.719-2.726 0-1.283-0.886-2.358-2.078-2.648zM8 13l-3-3h2v-3h2v3h2l-3 3z");
        model.commitTransaction("changegeo");

        model.startTransaction("changeStroke");
        model.setDataProperty(data, "stroke", "#888");
        model.commitTransaction("changeStroke");

        model.startTransaction("changeBackground");
        model.setDataProperty(data, "background", "#F7CA18");
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

        this.addDatas([{
            key: key2,
            text: "Process",
            stroke: "#888",
            background: "#E87E04",
            color: "#364150",
            category: "Default",
            touchPointId: id,
            connectionId: cid,
            geometryString: "M11.366 22.564l1.291-1.807-1.414-1.414-1.807 1.291c-0.335-0.187-0.694-0.337-1.071-0.444l-0.365-2.19h-2l-0.365 2.19c-0.377 0.107-0.736 0.256-1.071 0.444l-1.807-1.291-1.414 1.414 1.291 1.807c-0.187 0.335-0.337 0.694-0.443 1.071l-2.19 0.365v2l2.19 0.365c0.107 0.377 0.256 0.736 0.444 1.071l-1.291 1.807 1.414 1.414 1.807-1.291c0.335 0.187 0.694 0.337 1.071 0.444l0.365 2.19h2l0.365-2.19c0.377-0.107 0.736-0.256 1.071-0.444l1.807 1.291 1.414-1.414-1.291-1.807c0.187-0.335 0.337-0.694 0.444-1.071l2.19-0.365v-2l-2.19-0.365c-0.107-0.377-0.256-0.736-0.444-1.071zM7 27c-1.105 0-2-0.895-2-2s0.895-2 2-2 2 0.895 2 2-0.895 2-2 2zM32 12v-2l-2.106-0.383c-0.039-0.251-0.088-0.499-0.148-0.743l1.799-1.159-0.765-1.848-2.092 0.452c-0.132-0.216-0.273-0.426-0.422-0.629l1.219-1.761-1.414-1.414-1.761 1.219c-0.203-0.149-0.413-0.29-0.629-0.422l0.452-2.092-1.848-0.765-1.159 1.799c-0.244-0.059-0.492-0.109-0.743-0.148l-0.383-2.106h-2l-0.383 2.106c-0.251 0.039-0.499 0.088-0.743 0.148l-1.159-1.799-1.848 0.765 0.452 2.092c-0.216 0.132-0.426 0.273-0.629 0.422l-1.761-1.219-1.414 1.414 1.219 1.761c-0.149 0.203-0.29 0.413-0.422 0.629l-2.092-0.452-0.765 1.848 1.799 1.159c-0.059 0.244-0.109 0.492-0.148 0.743l-2.106 0.383v2l2.106 0.383c0.039 0.251 0.088 0.499 0.148 0.743l-1.799 1.159 0.765 1.848 2.092-0.452c0.132 0.216 0.273 0.426 0.422 0.629l-1.219 1.761 1.414 1.414 1.761-1.219c0.203 0.149 0.413 0.29 0.629 0.422l-0.452 2.092 1.848 0.765 1.159-1.799c0.244 0.059 0.492 0.109 0.743 0.148l0.383 2.106h2l0.383-2.106c0.251-0.039 0.499-0.088 0.743-0.148l1.159 1.799 1.848-0.765-0.452-2.092c0.216-0.132 0.426-0.273 0.629-0.422l1.761 1.219 1.414-1.414-1.219-1.761c0.149-0.203 0.29-0.413 0.422-0.629l2.092 0.452 0.765-1.848-1.799-1.159c0.059-0.244 0.109-0.492 0.148-0.743l2.106-0.383zM21 15.35c-2.402 0-4.35-1.948-4.35-4.35s1.948-4.35 4.35-4.35 4.35 1.948 4.35 4.35c0 2.402-1.948 4.35-4.35 4.35z",
            group: key1,
            type: 7,
           //loc: "50 0"

        }, {
            key: key3,
            text: "Post",
            stroke: "#888",
            background: "#F7CA18",
            color: "#364150",
            category: "Default",
            touchPointId: id,
            connectionId: cid,
            geometryString: "M13.942 6.039c0.038-0.174 0.058-0.354 0.058-0.539 0-1.381-1.119-2.5-2.5-2.5-0.222 0-0.438 0.029-0.643 0.084-0.387-1.209-1.52-2.084-2.857-2.084-1.365 0-2.516 0.911-2.88 2.159-0.355-0.103-0.731-0.159-1.12-0.159-2.209 0-4 1.791-4 4s1.791 4 4 4h2v3h4v-3h3.5c1.381 0 2.5-1.119 2.5-2.5 0-1.23-0.888-2.253-2.058-2.461zM9 10v3h-2v-3h-2.5l3.5-3.5 3.5 3.5h-2.5z",
            group: key1,
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



        this.addLinks([{ key: generateUUID(), from: key, to: key2 }, { key: generateUUID(), from: key2, to: key3 }]);
    };



    this.AddNodeTemplate = function (key, id, val, type, txt, shape, cid) {

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
        model.setDataProperty(data, "geometryString", shape);
        model.commitTransaction("changegeo");

        model.startTransaction("changegrp");
        model.setDataProperty(data, "group", key);
        model.commitTransaction("changegrp");

        model.startTransaction("changecategory");
        model.setDataProperty(data, "category", "Default");
        model.commitTransaction("changecategory");


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
        model.setDataProperty(data, "geometryString", shape);
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
        model.setDataProperty(data, "geometryString", "M4 4h10v24h-10zM18 4h10v24h-10z");
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
                background: "#3598DC",
                color: "#364150",
                category: "Default",
                geometryString: "M16 6h-6l2.243-2.243c-1.133-1.133-2.64-1.757-4.243-1.757s-3.109 0.624-4.243 1.757c-1.133 1.133-1.757 2.64-1.757 4.243s0.624 3.109 1.757 4.243c1.133 1.133 2.64 1.757 4.243 1.757s3.109-0.624 4.243-1.757c0.095-0.095 0.185-0.192 0.273-0.292l1.505 1.317c-1.466 1.674-3.62 2.732-6.020 2.732-4.418 0-8-3.582-8-8s3.582-8 8-8c2.209 0 4.209 0.896 5.656 2.344l2.343-2.344v6z",
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
            background: "#F7CA18",
            color: "#364150",
            category: "Default",
            touchPointId: id,
            connectionId: cid,
            geometryString: "M13.942 6.039c0.038-0.174 0.058-0.354 0.058-0.539 0-1.381-1.119-2.5-2.5-2.5-0.222 0-0.438 0.029-0.643 0.084-0.387-1.209-1.52-2.084-2.857-2.084-1.365 0-2.516 0.911-2.88 2.159-0.355-0.103-0.731-0.159-1.12-0.159-2.209 0-4 1.791-4 4s1.791 4 4 4h2v3h4v-3h3.5c1.381 0 2.5-1.119 2.5-2.5 0-1.23-0.888-2.253-2.058-2.461zM9 10v3h-2v-3h-2.5l3.5-3.5 3.5 3.5h-2.5z",
            group: groupKey,
            type: 6
        };
    };


    this.DataTemplate = function (entity, path, id) {



        var data = model.nodeDataArray[model.nodeDataArray.length - 1];



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

    this.changeSelfLoopLinkName = function (key, val) {

        var data = model.linkDataArray[model.linkDataArray.length - 1];  

        model.startTransaction("changeLinkName");
        model.setDataProperty(data, "ExecutionTime", val);
        model.commitTransaction("changeLinkName");

        model.startTransaction("changeUid");
        model.setDataProperty(data, "key", key);
        model.commitTransaction("changeUid");
    };


    this.getModelJson = function () {

        return model.toJson();

    };


    this.getCurrentNode = function () {
        return model.nodeDataArray[model.nodeDataArray.length - 1];
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
            $('#con-details').show();
            var data = (e instanceof go.Part) ? e.data : e;
            var taskId = data["touchPointId"];
            var connectionId = data["connectionId"];
            WorkFlow.findByTaskId(taskId, connectionId);
            var result = validateResult(data)
            $('.theme-panel').toggle(result)
            if (typeof u !== 'undefined') {
                data['isDirty'] = true;
                inspector.inspectObject(e);
                delete data.isDirty;
            }
        }
        else {
            $('.theme-panel').hide();
        }
        $('.toggler-close').trigger('click');
    };
    this.showIfExists = function (inspector, property) {

        var data = (inspector instanceof go.Part) ? inspector.data : inspector;
        if (typeof data[property] !== 'undefined')
            return true;
        return false;
    };

    inspector = new Inspector('myInfo', this.showInspectorHeader, this.graph,
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
              "BatchSize": { show: this.showIfExists, readOnly: false, type: 'number', pattern: '[0-9]', isInteger: true },
              "geometryString": { show: false },
              "ExecutionTime": { show: this.showIfExists, type: 'number', pattern: '[0-9]', isInteger: true },
              "isDirty": { show: false }
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