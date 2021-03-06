﻿#Region "Microsoft.VisualBasic::dde4888702c26005fbca6c0debcb403c, Dynamics\Engine\Driver.vb"

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

    '     Delegate Sub
    ' 
    ' 
    '     Interface IOmicsDataAdapter
    ' 
    '         Properties: mass
    ' 
    '         Sub: FluxSnapshot, MassSnapshot
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Engine

    ''' <summary>
    ''' Data storage snapshot driver
    ''' </summary>
    ''' <param name="iteration">Iteration number</param>
    ''' <param name="data">
    ''' Read snapshot data from the simulator engine
    ''' </param>
    Public Delegate Sub SnapshotDriver(iteration%, data As Dictionary(Of String, Double))

    ''' <summary>
    ''' Data storage adapter driver
    ''' </summary>
    Public Interface IOmicsDataAdapter

        ''' <summary>
        ''' The metabolite mass id index list
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property mass As OmicsTuple(Of String())

        Sub MassSnapshot(iteration As Integer, data As Dictionary(Of String, Double))
        Sub FluxSnapshot(iteration As Integer, data As Dictionary(Of String, Double))
    End Interface
End Namespace
