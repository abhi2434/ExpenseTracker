var templates = { 
    default: this.diagram(go.Node, "Spot", Graph.nodeStyle,


                    this.diagram(go.Panel, "Auto",
                      this.diagram(go.Shape,
                        new go.Binding("figure", "figure"),
                        new go.Binding("fill", "background"),
                        new go.Binding("stroke", "stroke")),
                      
                      this.diagram(go.TextBlock,
                        {
                            font: "bold 11pt Helvetica, Arial, sans-serif",
                            margin: 10,
                            maxSize: new go.Size(160, NaN),
                            wrap: go.TextBlock.WrapFit,
                            editable: false
                        },
                        new go.Binding("text", "text"),
                        new go.Binding("stroke", "color"),
                        new go.Binding("touchPointId", "tpId"))
                    ),

                     this.diagram("Button",
                {
                    alignment: go.Spot.TopRight,
                    "_buttonFillNormal": "#F7F7F7",
                    "_buttonStrokeNormal": "#808080",
                    "_buttonFillOver": "#F7F7F7",
                    "_buttonStrokeOver": "#808080"
                },
                this.diagram(go.Shape, "XLine", { width: 5, height: 5, stroke: "#ff0000" }),
                { click: deleteSegment }),
                    
                    makePort(this, "T", go.Spot.Top, false, true),
                    makePort(this, "B", go.Spot.Bottom, true, false)
                  ),

    textBottom: this.diagram(go.Node, "Vertical", Graph.nodeStyle,

                   
                        this.diagram(go.Panel, "Auto",
                          this.diagram(go.Shape,
                            new go.Binding("figure", "figure"),
                            new go.Binding("fill", "background"),
                            new go.Binding("stroke", "stroke")),
                        
                          this.diagram(go.TextBlock,
                            {
                                font: "bold 11pt Helvetica, Arial, sans-serif",
                                margin: 10,
                                maxSize: new go.Size(160, NaN),
                                wrap: go.TextBlock.WrapFit,
                                editable: false
                            },
                            new go.Binding("text", "text"),
                            new go.Binding("stroke", "color"),
                            new go.Binding("touchPointId", "tpId"))
                        ),

                         this.diagram("Button",
                    {
                        alignment: go.Spot.TopRight,
                        "_buttonFillNormal": "#F7F7F7",
                        "_buttonStrokeNormal": "#808080",
                        "_buttonFillOver": "#F7F7F7",
                        "_buttonStrokeOver": "#808080"
                    },
                    this.diagram(go.Shape, "XLine", { width: 5, height: 5, stroke: "#ff0000" }),
                    { click: deleteSegment }),
                        
                        makePort(this, "T", go.Spot.Top, false, true),
                        makePort(this, "B", go.Spot.Bottom, true, false)
                      ),

    start: this.diagram(go.Node, "Spot", Graph.nodeStyle,


                    this.diagram(go.Panel, "Auto",
                      this.diagram(go.Shape,
                        new go.Binding("figure", "figure"),
                        new go.Binding("fill", "background"),
                        new go.Binding("stroke", "stroke")),

                      this.diagram(go.TextBlock,
                        {
                            font: "bold 11pt Helvetica, Arial, sans-serif",
                            margin: 10,
                            maxSize: new go.Size(160, NaN),
                            wrap: go.TextBlock.WrapFit,
                            editable: false
                        },
                        new go.Binding("text", "text"),
                        new go.Binding("stroke", "color"),
                        new go.Binding("touchPointId", "tpId"))
                    ),

                     this.diagram("Button",
                {
                    alignment: go.Spot.TopRight,
                    "_buttonFillNormal": "#F7F7F7",
                    "_buttonStrokeNormal": "#808080",
                    "_buttonFillOver": "#F7F7F7",
                    "_buttonStrokeOver": "#808080"
                },
                this.diagram(go.Shape, "XLine", { width: 5, height: 5, stroke: "#ff0000" }),
                { click: deleteSegment }),
                    // four named ports, one on each side:

                    makePort(this, "B", go.Spot.Bottom, true, false)
                  ),

    stop:
                this.diagram(go.Node, "Spot", Graph.nodeStyle,


                  this.diagram(go.Panel, "Auto",
                    this.diagram(go.Shape,
                      new go.Binding("figure", "figure"),
                      new go.Binding("fill", "background"),
                      new go.Binding("stroke", "stroke")),
                    //this.diagram(go.Picture, {
                    //    margin: 2
                    //}, new go.Binding("source", "source"),
                    //  new go.Binding("width", "width"),
                    //  new go.Binding("height", "height")),
                    this.diagram(go.TextBlock,
                      {
                          font: "bold 11pt Helvetica, Arial, sans-serif",
                          margin: 10,
                          maxSize: new go.Size(160, NaN),
                          wrap: go.TextBlock.WrapFit,
                          editable: false
                      },
                      new go.Binding("text", "text"),
                      new go.Binding("stroke", "color"),
                      new go.Binding("touchPointId", "tpId"))
                  ),

                   this.diagram("Button",
              {
                  alignment: go.Spot.TopRight,
                  "_buttonFillNormal": "#F7F7F7",
                  "_buttonStrokeNormal": "#808080",
                  "_buttonFillOver": "#F7F7F7",
                  "_buttonStrokeOver": "#808080"
              },
              this.diagram(go.Shape, "XLine", { width: 5, height: 5, stroke: "#ff0000" }),
              { click: deleteSegment }),
                  // four named ports, one on each side:

                 makePort(this, "T", go.Spot.Top, false, true)
                )


};