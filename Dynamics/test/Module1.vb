﻿#Region "Microsoft.VisualBasic::9154af9ca9866179b4493dae1f93d81f, engine\Dynamics\test\Module1.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

' Module Module1
' 
'     Function: mass, reactions
' 
'     Sub: Main
' 
' /********************************************************************************/

#End Region

Imports Dynamics.Debugger
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.GCModeller.ModellingEngine.Dynamics.Core

Module Module1

    Sub Main()
        Dim massTable = mass.ToDictionary(Function(m) m.ID)
        Dim envir As New Vessel With {
            .MassEnvironment = massTable.Values.ToArray,
            .Channels = reactions(massTable).ToArray
        }

        Dim snapshots As New List(Of DataSet)
        Dim flux As New List(Of DataSet)

        Call envir.Initialize()

        For i As Integer = 0 To 100000
            flux += New DataSet With {
                .ID = i,
                .Properties = envir.ContainerIterator().ToDictionary.FlatTable
            }
            snapshots += New DataSet With {
                .ID = i,
                .Properties = massTable.ToDictionary(Function(m) m.Key, Function(m) m.Value.Value)
            }
        Next

        Call snapshots.SaveTo("./test_mass.csv")
        Call flux.SaveTo("./test_flux.csv")
        Call envir.ToGraph.DoCall(AddressOf Visualizer.CreateTabularFormat).Save("./test_network/")

        Pause()
    End Sub

    Private Iterator Function mass() As IEnumerable(Of Factor)
        Yield New Factor With {.ID = "A", .Value = 10}
        Yield New Factor With {.ID = "B", .Value = 1000}
        Yield New Factor With {.ID = "C", .Value = 10000}
        Yield New Factor With {.ID = "D", .Value = 1000}
        Yield New Factor With {.ID = "E", .Value = 5000}
        Yield New Factor With {.ID = "F", .Value = 1000}
        Yield New Factor With {.ID = "G", .Value = 3000}
        Yield New Factor With {.ID = "H", .Value = 1000}
        Yield New Factor With {.ID = "I", .Value = 8000}
        Yield New Factor With {.ID = "J", .Value = 2000}
    End Function

    ''' <summary>
    ''' Build a test network
    ''' </summary>
    ''' <param name="massTable"></param>
    ''' <returns></returns>
    Private Iterator Function reactions(massTable As Dictionary(Of String, Factor)) As IEnumerable(Of Channel)
        Dim pop = Iterator Function(names As String()) As IEnumerable(Of Variable)
                      For Each ref In names
                          Yield New Variable(massTable(ref), 1)
                      Next
                  End Function

        Yield New Channel(pop({"A", "B"}), pop({"C", "D"})) With {
            .bounds = {50, 50},
            .ID = "ABCD",
            .forward = New Controls,
            .reverse = New Controls With {.activation = pop({"B", "D"}).ToArray}}

        Yield New Channel(pop({"E", "F"}), pop({"A", "G"})) With {
            .bounds = {50, 50},
            .ID = "EFAG",
            .forward = New Controls,
            .reverse = New Controls With {.activation = pop({"B"}).ToArray}
        }

        Yield New Channel(pop({"B"}), pop({"A", "D"})) With {
            .bounds = {50, 50},
            .ID = "BAD",
            .forward = New Controls With {.activation = pop({"C", "G", "B"}).ToArray},
            .reverse = New Controls With {.activation = pop({"E"}).ToArray}
        }

        Yield New Channel(pop({"G"}), pop({"E"})) With {
            .bounds = {50, 50},
            .ID = "GE",
            .forward = New Controls With {.activation = pop({"F"}).ToArray}
        }
        Yield New Channel(pop({"E"}), pop({"G", "D", "C"})) With {
            .bounds = {50, 50},
            .ID = "EGDC",
            .forward = New Controls With {.activation = pop({"E"}).ToArray},
            .reverse = New Controls With {.activation = pop({"C", "D"}).ToArray}
        }

        Yield New Channel(pop({"B", "F"}), pop({"H"})) With {
            .bounds = {50, 50},
            .ID = "BFH",
            .forward = New Controls With {.activation = pop({"B"}).ToArray},
            .reverse = New Controls With {.activation = pop({"I", "D"}).ToArray}
        }

        Yield New Channel(pop({"D", "F"}), pop({"H"})) With {
            .bounds = {50, 50},
            .ID = "DFH",
            .forward = New Controls With {.activation = pop({"B"}).ToArray},
            .reverse = New Controls With {.activation = pop({"I", "D"}).ToArray}
        }

        Yield New Channel(pop({"I"}), pop({"G"})) With {
            .bounds = {50, 50},
            .ID = "IG",
           .forward = New Controls With {.activation = pop({"B"}).ToArray},
           .reverse = New Controls With {.activation = pop({"G", "D"}).ToArray}
       }

        Yield New Channel(pop({"H"}), pop({"I", "D"})) With {
            .bounds = {50, 50},
            .ID = "HID",
           .forward = New Controls With {.activation = pop({"B", "H"}).ToArray},
           .reverse = New Controls With {.activation = pop({"A"}).ToArray}
       }

        Yield New Channel(pop({"I", "D"}), pop({"J", "B"})) With {
            .bounds = {50, 50},
            .ID = "IDJB",
           .forward = New Controls With {.activation = pop({"A"}).ToArray},
           .reverse = New Controls With {.activation = pop({"C"}).ToArray}
       }
    End Function
End Module
