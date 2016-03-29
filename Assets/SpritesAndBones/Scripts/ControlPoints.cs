/*
The MIT License (MIT)

Copyright (c) 2016 Play-Em

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions)

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

public class ControlPoints : MonoBehaviour {

	[HideInInspector]
	public Vector3 cp, cp1, cp2, cp3, cp4, cp5, cp6, cp7, cp8, cp9, cp10, cp11, cp12, cp13, cp14, cp15, cp16, cp17, cp18, cp19, cp20, cp21, cp22, cp23, cp24, cp25, cp26, cp27, cp28, cp29, cp30, cp31, cp32, cp33, cp34, cp35, cp36, cp37, cp38, cp39, cp40, cp41, cp42, cp43, cp44, cp45, cp46, cp47, cp48, cp49, cp50, cp51, cp52, cp53, cp54, cp55, cp56, cp57, cp58, cp59, cp60, cp61, cp62, cp63, cp64, cp65, cp66, cp67, cp68, cp69, cp70, cp71, cp72, cp73, cp74, cp75, cp76, cp77, cp78, cp79, cp80, cp81, cp82, cp83, cp84, cp85, cp86, cp87, cp88, cp89, cp90, cp91, cp92, cp93, cp94, cp95, cp96, cp97, cp98, cp99, cp100, cp101, cp102, cp103, cp104, cp105, cp106, cp107, cp108, cp109, cp110, cp111, cp112, cp113, cp114, cp115, cp116, cp117, cp118, cp119, cp120, cp121, cp122, cp123, cp124, cp125, cp126, cp127, cp128, cp129, cp130, cp131, cp132, cp133, cp134, cp135, cp136, cp137, cp138, cp139, cp140, cp141, cp142, cp143, cp144, cp145, cp146, cp147, cp148, cp149, cp150, cp151, cp152, cp153, cp154, cp155, cp156, cp157, cp158, cp159, cp160, cp161, cp162, cp163, cp164, cp165, cp166, cp167, cp168, cp169, cp170, cp171, cp172, cp173, cp174, cp175, cp176, cp177, cp178, cp179, cp180, cp181, cp182, cp183, cp184, cp185, cp186, cp187, cp188, cp189, cp190, cp191, cp192, cp193, cp194, cp195, cp196, cp197, cp198, cp199, cp200, cp201, cp202, cp203, cp204, cp205, cp206, cp207, cp208, cp209, cp210, cp211, cp212, cp213, cp214, cp215, cp216, cp217, cp218, cp219, cp220, cp221, cp222, cp223, cp224, cp225, cp226, cp227, cp228, cp229, cp230, cp231, cp232, cp233, cp234, cp235, cp236, cp237, cp238, cp239, cp240, cp241, cp242, cp243, cp244, cp245, cp246, cp247, cp248, cp249, cp250, cp251, cp252, cp253, cp254, cp255, cp256, cp257, cp258, cp259, cp260, cp261, cp262, cp263, cp264, cp265, cp266, cp267, cp268, cp269, cp270, cp271, cp272, cp273, cp274, cp275, cp276, cp277, cp278, cp279, cp280, cp281, cp282, cp283, cp284, cp285, cp286, cp287, cp288, cp289, cp290, cp291, cp292, cp293, cp294, cp295, cp296, cp297, cp298, cp299, cp300, cp301, cp302, cp303, cp304, cp305, cp306, cp307, cp308, cp309, cp310, cp311, cp312, cp313, cp314, cp315, cp316, cp317, cp318, cp319, cp320, cp321, cp322, cp323, cp324, cp325, cp326, cp327, cp328, cp329, cp330, cp331, cp332, cp333, cp334, cp335, cp336, cp337, cp338, cp339, cp340, cp341, cp342, cp343, cp344, cp345, cp346, cp347, cp348, cp349, cp350, cp351, cp352, cp353, cp354, cp355, cp356, cp357, cp358, cp359, cp360, cp361, cp362, cp363, cp364, cp365, cp366, cp367, cp368, cp369, cp370, cp371, cp372, cp373, cp374, cp375, cp376, cp377, cp378, cp379, cp380, cp381, cp382, cp383, cp384, cp385, cp386, cp387, cp388, cp389, cp390, cp391, cp392, cp393, cp394, cp395, cp396, cp397, cp398, cp399, cp400, cp401, cp402, cp403, cp404, cp405, cp406, cp407, cp408, cp409, cp410, cp411, cp412, cp413, cp414, cp415, cp416, cp417, cp418, cp419, cp420, cp421, cp422, cp423, cp424, cp425, cp426, cp427, cp428, cp429, cp430, cp431, cp432, cp433, cp434, cp435, cp436, cp437, cp438, cp439, cp440, cp441, cp442, cp443, cp444, cp445, cp446, cp447, cp448, cp449, cp450, cp451, cp452, cp453, cp454, cp455, cp456, cp457, cp458, cp459, cp460, cp461, cp462, cp463, cp464, cp465, cp466, cp467, cp468, cp469, cp470, cp471, cp472, cp473, cp474, cp475, cp476, cp477, cp478, cp479, cp480, cp481, cp482, cp483, cp484, cp485, cp486, cp487, cp488, cp489, cp490, cp491, cp492, cp493, cp494, cp495, cp496, cp497, cp498, cp499, cp500, cp501, cp502, cp503, cp504, cp505, cp506, cp507, cp508, cp509, cp510, cp511;

	public void SetPoint(Point point) {
		#if UNITY_EDITOR
		Undo.RecordObject(this, "Changed Control Points");
		#endif
		if (point.index == 0) { cp = point.position; }
		else if (point.index == 1) { cp1 = point.position; }
		else if (point.index == 2) { cp2 = point.position; }
		else if (point.index == 3) { cp3 = point.position; }
		else if (point.index == 4) { cp4 = point.position; }
		else if (point.index == 5) { cp5 = point.position; }
		else if (point.index == 6) { cp6 = point.position; }
		else if (point.index == 7) { cp7 = point.position; }
		else if (point.index == 8) { cp8 = point.position; }
		else if (point.index == 9) { cp9 = point.position; }
		else if (point.index == 10) { cp10 = point.position; }
		else if (point.index == 11) { cp11 = point.position; }
		else if (point.index == 12) { cp12 = point.position; }
		else if (point.index == 13) { cp13 = point.position; }
		else if (point.index == 14) { cp14 = point.position; }
		else if (point.index == 15) { cp15 = point.position; }
		else if (point.index == 16) { cp16 = point.position; }
		else if (point.index == 17) { cp17 = point.position; }
		else if (point.index == 18) { cp18 = point.position; }
		else if (point.index == 19) { cp19 = point.position; }
		else if (point.index == 20) { cp20 = point.position; }
		else if (point.index == 21) { cp21 = point.position; }
		else if (point.index == 22) { cp22 = point.position; }
		else if (point.index == 23) { cp23 = point.position; }
		else if (point.index == 24) { cp24 = point.position; }
		else if (point.index == 25) { cp25 = point.position; }
		else if (point.index == 26) { cp26 = point.position; }
		else if (point.index == 27) { cp27 = point.position; }
		else if (point.index == 28) { cp28 = point.position; }
		else if (point.index == 29) { cp29 = point.position; }
		else if (point.index == 30) { cp30 = point.position; }
		else if (point.index == 31) { cp31 = point.position; }
		else if (point.index == 32) { cp32 = point.position; }
		else if (point.index == 33) { cp33 = point.position; }
		else if (point.index == 34) { cp34 = point.position; }
		else if (point.index == 35) { cp35 = point.position; }
		else if (point.index == 36) { cp36 = point.position; }
		else if (point.index == 37) { cp37 = point.position; }
		else if (point.index == 38) { cp38 = point.position; }
		else if (point.index == 39) { cp39 = point.position; }
		else if (point.index == 40) { cp40 = point.position; }
		else if (point.index == 41) { cp41 = point.position; }
		else if (point.index == 42) { cp42 = point.position; }
		else if (point.index == 43) { cp43 = point.position; }
		else if (point.index == 44) { cp44 = point.position; }
		else if (point.index == 45) { cp45 = point.position; }
		else if (point.index == 46) { cp46 = point.position; }
		else if (point.index == 47) { cp47 = point.position; }
		else if (point.index == 48) { cp48 = point.position; }
		else if (point.index == 49) { cp49 = point.position; }
		else if (point.index == 50) { cp50 = point.position; }
		else if (point.index == 51) { cp51 = point.position; }
		else if (point.index == 52) { cp52 = point.position; }
		else if (point.index == 53) { cp53 = point.position; }
		else if (point.index == 54) { cp54 = point.position; }
		else if (point.index == 55) { cp55 = point.position; }
		else if (point.index == 56) { cp56 = point.position; }
		else if (point.index == 57) { cp57 = point.position; }
		else if (point.index == 58) { cp58 = point.position; }
		else if (point.index == 59) { cp59 = point.position; }
		else if (point.index == 60) { cp60 = point.position; }
		else if (point.index == 61) { cp61 = point.position; }
		else if (point.index == 62) { cp62 = point.position; }
		else if (point.index == 63) { cp63 = point.position; }
		else if (point.index == 64) { cp64 = point.position; }
		else if (point.index == 65) { cp65 = point.position; }
		else if (point.index == 66) { cp66 = point.position; }
		else if (point.index == 67) { cp67 = point.position; }
		else if (point.index == 68) { cp68 = point.position; }
		else if (point.index == 69) { cp69 = point.position; }
		else if (point.index == 70) { cp70 = point.position; }
		else if (point.index == 71) { cp71 = point.position; }
		else if (point.index == 72) { cp72 = point.position; }
		else if (point.index == 73) { cp73 = point.position; }
		else if (point.index == 74) { cp74 = point.position; }
		else if (point.index == 75) { cp75 = point.position; }
		else if (point.index == 76) { cp76 = point.position; }
		else if (point.index == 77) { cp77 = point.position; }
		else if (point.index == 78) { cp78 = point.position; }
		else if (point.index == 79) { cp79 = point.position; }
		else if (point.index == 80) { cp80 = point.position; }
		else if (point.index == 81) { cp81 = point.position; }
		else if (point.index == 82) { cp82 = point.position; }
		else if (point.index == 83) { cp83 = point.position; }
		else if (point.index == 84) { cp84 = point.position; }
		else if (point.index == 85) { cp85 = point.position; }
		else if (point.index == 86) { cp86 = point.position; }
		else if (point.index == 87) { cp87 = point.position; }
		else if (point.index == 88) { cp88 = point.position; }
		else if (point.index == 89) { cp89 = point.position; }
		else if (point.index == 90) { cp90 = point.position; }
		else if (point.index == 91) { cp91 = point.position; }
		else if (point.index == 92) { cp92 = point.position; }
		else if (point.index == 93) { cp93 = point.position; }
		else if (point.index == 94) { cp94 = point.position; }
		else if (point.index == 95) { cp95 = point.position; }
		else if (point.index == 96) { cp96 = point.position; }
		else if (point.index == 97) { cp97 = point.position; }
		else if (point.index == 98) { cp98 = point.position; }
		else if (point.index == 99) { cp99 = point.position; }
		else if (point.index == 100) { cp100 = point.position; }
		else if (point.index == 101) { cp101 = point.position; }
		else if (point.index == 102) { cp102 = point.position; }
		else if (point.index == 103) { cp103 = point.position; }
		else if (point.index == 104) { cp104 = point.position; }
		else if (point.index == 105) { cp105 = point.position; }
		else if (point.index == 106) { cp106 = point.position; }
		else if (point.index == 107) { cp107 = point.position; }
		else if (point.index == 108) { cp108 = point.position; }
		else if (point.index == 109) { cp109 = point.position; }
		else if (point.index == 110) { cp110 = point.position; }
		else if (point.index == 111) { cp111 = point.position; }
		else if (point.index == 112) { cp112 = point.position; }
		else if (point.index == 113) { cp113 = point.position; }
		else if (point.index == 114) { cp114 = point.position; }
		else if (point.index == 115) { cp115 = point.position; }
		else if (point.index == 116) { cp116 = point.position; }
		else if (point.index == 117) { cp117 = point.position; }
		else if (point.index == 118) { cp118 = point.position; }
		else if (point.index == 119) { cp119 = point.position; }
		else if (point.index == 120) { cp120 = point.position; }
		else if (point.index == 121) { cp121 = point.position; }
		else if (point.index == 122) { cp122 = point.position; }
		else if (point.index == 123) { cp123 = point.position; }
		else if (point.index == 124) { cp124 = point.position; }
		else if (point.index == 125) { cp125 = point.position; }
		else if (point.index == 126) { cp126 = point.position; }
		else if (point.index == 127) { cp127 = point.position; }
		else if (point.index == 128) { cp128 = point.position; }
		else if (point.index == 129) { cp129 = point.position; }
		else if (point.index == 130) { cp130 = point.position; }
		else if (point.index == 131) { cp131 = point.position; }
		else if (point.index == 132) { cp132 = point.position; }
		else if (point.index == 133) { cp133 = point.position; }
		else if (point.index == 134) { cp134 = point.position; }
		else if (point.index == 135) { cp135 = point.position; }
		else if (point.index == 136) { cp136 = point.position; }
		else if (point.index == 137) { cp137 = point.position; }
		else if (point.index == 138) { cp138 = point.position; }
		else if (point.index == 139) { cp139 = point.position; }
		else if (point.index == 140) { cp140 = point.position; }
		else if (point.index == 141) { cp141 = point.position; }
		else if (point.index == 142) { cp142 = point.position; }
		else if (point.index == 143) { cp143 = point.position; }
		else if (point.index == 144) { cp144 = point.position; }
		else if (point.index == 145) { cp145 = point.position; }
		else if (point.index == 146) { cp146 = point.position; }
		else if (point.index == 147) { cp147 = point.position; }
		else if (point.index == 148) { cp148 = point.position; }
		else if (point.index == 149) { cp149 = point.position; }
		else if (point.index == 150) { cp150 = point.position; }
		else if (point.index == 151) { cp151 = point.position; }
		else if (point.index == 152) { cp152 = point.position; }
		else if (point.index == 153) { cp153 = point.position; }
		else if (point.index == 154) { cp154 = point.position; }
		else if (point.index == 155) { cp155 = point.position; }
		else if (point.index == 156) { cp156 = point.position; }
		else if (point.index == 157) { cp157 = point.position; }
		else if (point.index == 158) { cp158 = point.position; }
		else if (point.index == 159) { cp159 = point.position; }
		else if (point.index == 160) { cp160 = point.position; }
		else if (point.index == 161) { cp161 = point.position; }
		else if (point.index == 162) { cp162 = point.position; }
		else if (point.index == 163) { cp163 = point.position; }
		else if (point.index == 164) { cp164 = point.position; }
		else if (point.index == 165) { cp165 = point.position; }
		else if (point.index == 166) { cp166 = point.position; }
		else if (point.index == 167) { cp167 = point.position; }
		else if (point.index == 168) { cp168 = point.position; }
		else if (point.index == 169) { cp169 = point.position; }
		else if (point.index == 170) { cp170 = point.position; }
		else if (point.index == 171) { cp171 = point.position; }
		else if (point.index == 172) { cp172 = point.position; }
		else if (point.index == 173) { cp173 = point.position; }
		else if (point.index == 174) { cp174 = point.position; }
		else if (point.index == 175) { cp175 = point.position; }
		else if (point.index == 176) { cp176 = point.position; }
		else if (point.index == 177) { cp177 = point.position; }
		else if (point.index == 178) { cp178 = point.position; }
		else if (point.index == 179) { cp179 = point.position; }
		else if (point.index == 180) { cp180 = point.position; }
		else if (point.index == 181) { cp181 = point.position; }
		else if (point.index == 182) { cp182 = point.position; }
		else if (point.index == 183) { cp183 = point.position; }
		else if (point.index == 184) { cp184 = point.position; }
		else if (point.index == 185) { cp185 = point.position; }
		else if (point.index == 186) { cp186 = point.position; }
		else if (point.index == 187) { cp187 = point.position; }
		else if (point.index == 188) { cp188 = point.position; }
		else if (point.index == 189) { cp189 = point.position; }
		else if (point.index == 190) { cp190 = point.position; }
		else if (point.index == 191) { cp191 = point.position; }
		else if (point.index == 192) { cp192 = point.position; }
		else if (point.index == 193) { cp193 = point.position; }
		else if (point.index == 194) { cp194 = point.position; }
		else if (point.index == 195) { cp195 = point.position; }
		else if (point.index == 196) { cp196 = point.position; }
		else if (point.index == 197) { cp197 = point.position; }
		else if (point.index == 198) { cp198 = point.position; }
		else if (point.index == 199) { cp199 = point.position; }
		else if (point.index == 200) { cp200 = point.position; }
		else if (point.index == 201) { cp201 = point.position; }
		else if (point.index == 202) { cp202 = point.position; }
		else if (point.index == 203) { cp203 = point.position; }
		else if (point.index == 204) { cp204 = point.position; }
		else if (point.index == 205) { cp205 = point.position; }
		else if (point.index == 206) { cp206 = point.position; }
		else if (point.index == 207) { cp207 = point.position; }
		else if (point.index == 208) { cp208 = point.position; }
		else if (point.index == 209) { cp209 = point.position; }
		else if (point.index == 210) { cp210 = point.position; }
		else if (point.index == 211) { cp211 = point.position; }
		else if (point.index == 212) { cp212 = point.position; }
		else if (point.index == 213) { cp213 = point.position; }
		else if (point.index == 214) { cp214 = point.position; }
		else if (point.index == 215) { cp215 = point.position; }
		else if (point.index == 216) { cp216 = point.position; }
		else if (point.index == 217) { cp217 = point.position; }
		else if (point.index == 218) { cp218 = point.position; }
		else if (point.index == 219) { cp219 = point.position; }
		else if (point.index == 220) { cp220 = point.position; }
		else if (point.index == 221) { cp221 = point.position; }
		else if (point.index == 222) { cp222 = point.position; }
		else if (point.index == 223) { cp223 = point.position; }
		else if (point.index == 224) { cp224 = point.position; }
		else if (point.index == 225) { cp225 = point.position; }
		else if (point.index == 226) { cp226 = point.position; }
		else if (point.index == 227) { cp227 = point.position; }
		else if (point.index == 228) { cp228 = point.position; }
		else if (point.index == 229) { cp229 = point.position; }
		else if (point.index == 230) { cp230 = point.position; }
		else if (point.index == 231) { cp231 = point.position; }
		else if (point.index == 232) { cp232 = point.position; }
		else if (point.index == 233) { cp233 = point.position; }
		else if (point.index == 234) { cp234 = point.position; }
		else if (point.index == 235) { cp235 = point.position; }
		else if (point.index == 236) { cp236 = point.position; }
		else if (point.index == 237) { cp237 = point.position; }
		else if (point.index == 238) { cp238 = point.position; }
		else if (point.index == 239) { cp239 = point.position; }
		else if (point.index == 240) { cp240 = point.position; }
		else if (point.index == 241) { cp241 = point.position; }
		else if (point.index == 242) { cp242 = point.position; }
		else if (point.index == 243) { cp243 = point.position; }
		else if (point.index == 244) { cp244 = point.position; }
		else if (point.index == 245) { cp245 = point.position; }
		else if (point.index == 246) { cp246 = point.position; }
		else if (point.index == 247) { cp247 = point.position; }
		else if (point.index == 248) { cp248 = point.position; }
		else if (point.index == 249) { cp249 = point.position; }
		else if (point.index == 250) { cp250 = point.position; }
		else if (point.index == 251) { cp251 = point.position; }
		else if (point.index == 252) { cp252 = point.position; }
		else if (point.index == 253) { cp253 = point.position; }
		else if (point.index == 254) { cp254 = point.position; }
		else if (point.index == 255) { cp255 = point.position; }
		else if (point.index == 256) { cp256 = point.position; }
		else if (point.index == 257) { cp257 = point.position; }
		else if (point.index == 258) { cp258 = point.position; }
		else if (point.index == 259) { cp259 = point.position; }
		else if (point.index == 260) { cp260 = point.position; }
		else if (point.index == 261) { cp261 = point.position; }
		else if (point.index == 262) { cp262 = point.position; }
		else if (point.index == 263) { cp263 = point.position; }
		else if (point.index == 264) { cp264 = point.position; }
		else if (point.index == 265) { cp265 = point.position; }
		else if (point.index == 266) { cp266 = point.position; }
		else if (point.index == 267) { cp267 = point.position; }
		else if (point.index == 268) { cp268 = point.position; }
		else if (point.index == 269) { cp269 = point.position; }
		else if (point.index == 270) { cp270 = point.position; }
		else if (point.index == 271) { cp271 = point.position; }
		else if (point.index == 272) { cp272 = point.position; }
		else if (point.index == 273) { cp273 = point.position; }
		else if (point.index == 274) { cp274 = point.position; }
		else if (point.index == 275) { cp275 = point.position; }
		else if (point.index == 276) { cp276 = point.position; }
		else if (point.index == 277) { cp277 = point.position; }
		else if (point.index == 278) { cp278 = point.position; }
		else if (point.index == 279) { cp279 = point.position; }
		else if (point.index == 280) { cp280 = point.position; }
		else if (point.index == 281) { cp281 = point.position; }
		else if (point.index == 282) { cp282 = point.position; }
		else if (point.index == 283) { cp283 = point.position; }
		else if (point.index == 284) { cp284 = point.position; }
		else if (point.index == 285) { cp285 = point.position; }
		else if (point.index == 286) { cp286 = point.position; }
		else if (point.index == 287) { cp287 = point.position; }
		else if (point.index == 288) { cp288 = point.position; }
		else if (point.index == 289) { cp289 = point.position; }
		else if (point.index == 290) { cp290 = point.position; }
		else if (point.index == 291) { cp291 = point.position; }
		else if (point.index == 292) { cp292 = point.position; }
		else if (point.index == 293) { cp293 = point.position; }
		else if (point.index == 294) { cp294 = point.position; }
		else if (point.index == 295) { cp295 = point.position; }
		else if (point.index == 296) { cp296 = point.position; }
		else if (point.index == 297) { cp297 = point.position; }
		else if (point.index == 298) { cp298 = point.position; }
		else if (point.index == 299) { cp299 = point.position; }
		else if (point.index == 300) { cp300 = point.position; }
		else if (point.index == 301) { cp301 = point.position; }
		else if (point.index == 302) { cp302 = point.position; }
		else if (point.index == 303) { cp303 = point.position; }
		else if (point.index == 304) { cp304 = point.position; }
		else if (point.index == 305) { cp305 = point.position; }
		else if (point.index == 306) { cp306 = point.position; }
		else if (point.index == 307) { cp307 = point.position; }
		else if (point.index == 308) { cp308 = point.position; }
		else if (point.index == 309) { cp309 = point.position; }
		else if (point.index == 310) { cp310 = point.position; }
		else if (point.index == 311) { cp311 = point.position; }
		else if (point.index == 312) { cp312 = point.position; }
		else if (point.index == 313) { cp313 = point.position; }
		else if (point.index == 314) { cp314 = point.position; }
		else if (point.index == 315) { cp315 = point.position; }
		else if (point.index == 316) { cp316 = point.position; }
		else if (point.index == 317) { cp317 = point.position; }
		else if (point.index == 318) { cp318 = point.position; }
		else if (point.index == 319) { cp319 = point.position; }
		else if (point.index == 320) { cp320 = point.position; }
		else if (point.index == 321) { cp321 = point.position; }
		else if (point.index == 322) { cp322 = point.position; }
		else if (point.index == 323) { cp323 = point.position; }
		else if (point.index == 324) { cp324 = point.position; }
		else if (point.index == 325) { cp325 = point.position; }
		else if (point.index == 326) { cp326 = point.position; }
		else if (point.index == 327) { cp327 = point.position; }
		else if (point.index == 328) { cp328 = point.position; }
		else if (point.index == 329) { cp329 = point.position; }
		else if (point.index == 330) { cp330 = point.position; }
		else if (point.index == 331) { cp331 = point.position; }
		else if (point.index == 332) { cp332 = point.position; }
		else if (point.index == 333) { cp333 = point.position; }
		else if (point.index == 334) { cp334 = point.position; }
		else if (point.index == 335) { cp335 = point.position; }
		else if (point.index == 336) { cp336 = point.position; }
		else if (point.index == 337) { cp337 = point.position; }
		else if (point.index == 338) { cp338 = point.position; }
		else if (point.index == 339) { cp339 = point.position; }
		else if (point.index == 340) { cp340 = point.position; }
		else if (point.index == 341) { cp341 = point.position; }
		else if (point.index == 342) { cp342 = point.position; }
		else if (point.index == 343) { cp343 = point.position; }
		else if (point.index == 344) { cp344 = point.position; }
		else if (point.index == 345) { cp345 = point.position; }
		else if (point.index == 346) { cp346 = point.position; }
		else if (point.index == 347) { cp347 = point.position; }
		else if (point.index == 348) { cp348 = point.position; }
		else if (point.index == 349) { cp349 = point.position; }
		else if (point.index == 350) { cp350 = point.position; }
		else if (point.index == 351) { cp351 = point.position; }
		else if (point.index == 352) { cp352 = point.position; }
		else if (point.index == 353) { cp353 = point.position; }
		else if (point.index == 354) { cp354 = point.position; }
		else if (point.index == 355) { cp355 = point.position; }
		else if (point.index == 356) { cp356 = point.position; }
		else if (point.index == 357) { cp357 = point.position; }
		else if (point.index == 358) { cp358 = point.position; }
		else if (point.index == 359) { cp359 = point.position; }
		else if (point.index == 360) { cp360 = point.position; }
		else if (point.index == 361) { cp361 = point.position; }
		else if (point.index == 362) { cp362 = point.position; }
		else if (point.index == 363) { cp363 = point.position; }
		else if (point.index == 364) { cp364 = point.position; }
		else if (point.index == 365) { cp365 = point.position; }
		else if (point.index == 366) { cp366 = point.position; }
		else if (point.index == 367) { cp367 = point.position; }
		else if (point.index == 368) { cp368 = point.position; }
		else if (point.index == 369) { cp369 = point.position; }
		else if (point.index == 370) { cp370 = point.position; }
		else if (point.index == 371) { cp371 = point.position; }
		else if (point.index == 372) { cp372 = point.position; }
		else if (point.index == 373) { cp373 = point.position; }
		else if (point.index == 374) { cp374 = point.position; }
		else if (point.index == 375) { cp375 = point.position; }
		else if (point.index == 376) { cp376 = point.position; }
		else if (point.index == 377) { cp377 = point.position; }
		else if (point.index == 378) { cp378 = point.position; }
		else if (point.index == 379) { cp379 = point.position; }
		else if (point.index == 380) { cp380 = point.position; }
		else if (point.index == 381) { cp381 = point.position; }
		else if (point.index == 382) { cp382 = point.position; }
		else if (point.index == 383) { cp383 = point.position; }
		else if (point.index == 384) { cp384 = point.position; }
		else if (point.index == 385) { cp385 = point.position; }
		else if (point.index == 386) { cp386 = point.position; }
		else if (point.index == 387) { cp387 = point.position; }
		else if (point.index == 388) { cp388 = point.position; }
		else if (point.index == 389) { cp389 = point.position; }
		else if (point.index == 390) { cp390 = point.position; }
		else if (point.index == 391) { cp391 = point.position; }
		else if (point.index == 392) { cp392 = point.position; }
		else if (point.index == 393) { cp393 = point.position; }
		else if (point.index == 394) { cp394 = point.position; }
		else if (point.index == 395) { cp395 = point.position; }
		else if (point.index == 396) { cp396 = point.position; }
		else if (point.index == 397) { cp397 = point.position; }
		else if (point.index == 398) { cp398 = point.position; }
		else if (point.index == 399) { cp399 = point.position; }
		else if (point.index == 400) { cp400 = point.position; }
		else if (point.index == 401) { cp401 = point.position; }
		else if (point.index == 402) { cp402 = point.position; }
		else if (point.index == 403) { cp403 = point.position; }
		else if (point.index == 404) { cp404 = point.position; }
		else if (point.index == 405) { cp405 = point.position; }
		else if (point.index == 406) { cp406 = point.position; }
		else if (point.index == 407) { cp407 = point.position; }
		else if (point.index == 408) { cp408 = point.position; }
		else if (point.index == 409) { cp409 = point.position; }
		else if (point.index == 410) { cp410 = point.position; }
		else if (point.index == 411) { cp411 = point.position; }
		else if (point.index == 412) { cp412 = point.position; }
		else if (point.index == 413) { cp413 = point.position; }
		else if (point.index == 414) { cp414 = point.position; }
		else if (point.index == 415) { cp415 = point.position; }
		else if (point.index == 416) { cp416 = point.position; }
		else if (point.index == 417) { cp417 = point.position; }
		else if (point.index == 418) { cp418 = point.position; }
		else if (point.index == 419) { cp419 = point.position; }
		else if (point.index == 420) { cp420 = point.position; }
		else if (point.index == 421) { cp421 = point.position; }
		else if (point.index == 422) { cp422 = point.position; }
		else if (point.index == 423) { cp423 = point.position; }
		else if (point.index == 424) { cp424 = point.position; }
		else if (point.index == 425) { cp425 = point.position; }
		else if (point.index == 426) { cp426 = point.position; }
		else if (point.index == 427) { cp427 = point.position; }
		else if (point.index == 428) { cp428 = point.position; }
		else if (point.index == 429) { cp429 = point.position; }
		else if (point.index == 430) { cp430 = point.position; }
		else if (point.index == 431) { cp431 = point.position; }
		else if (point.index == 432) { cp432 = point.position; }
		else if (point.index == 433) { cp433 = point.position; }
		else if (point.index == 434) { cp434 = point.position; }
		else if (point.index == 435) { cp435 = point.position; }
		else if (point.index == 436) { cp436 = point.position; }
		else if (point.index == 437) { cp437 = point.position; }
		else if (point.index == 438) { cp438 = point.position; }
		else if (point.index == 439) { cp439 = point.position; }
		else if (point.index == 440) { cp440 = point.position; }
		else if (point.index == 441) { cp441 = point.position; }
		else if (point.index == 442) { cp442 = point.position; }
		else if (point.index == 443) { cp443 = point.position; }
		else if (point.index == 444) { cp444 = point.position; }
		else if (point.index == 445) { cp445 = point.position; }
		else if (point.index == 446) { cp446 = point.position; }
		else if (point.index == 447) { cp447 = point.position; }
		else if (point.index == 448) { cp448 = point.position; }
		else if (point.index == 449) { cp449 = point.position; }
		else if (point.index == 450) { cp450 = point.position; }
		else if (point.index == 451) { cp451 = point.position; }
		else if (point.index == 452) { cp452 = point.position; }
		else if (point.index == 453) { cp453 = point.position; }
		else if (point.index == 454) { cp454 = point.position; }
		else if (point.index == 455) { cp455 = point.position; }
		else if (point.index == 456) { cp456 = point.position; }
		else if (point.index == 457) { cp457 = point.position; }
		else if (point.index == 458) { cp458 = point.position; }
		else if (point.index == 459) { cp459 = point.position; }
		else if (point.index == 460) { cp460 = point.position; }
		else if (point.index == 461) { cp461 = point.position; }
		else if (point.index == 462) { cp462 = point.position; }
		else if (point.index == 463) { cp463 = point.position; }
		else if (point.index == 464) { cp464 = point.position; }
		else if (point.index == 465) { cp465 = point.position; }
		else if (point.index == 466) { cp466 = point.position; }
		else if (point.index == 467) { cp467 = point.position; }
		else if (point.index == 468) { cp468 = point.position; }
		else if (point.index == 469) { cp469 = point.position; }
		else if (point.index == 470) { cp470 = point.position; }
		else if (point.index == 471) { cp471 = point.position; }
		else if (point.index == 472) { cp472 = point.position; }
		else if (point.index == 473) { cp473 = point.position; }
		else if (point.index == 474) { cp474 = point.position; }
		else if (point.index == 475) { cp475 = point.position; }
		else if (point.index == 476) { cp476 = point.position; }
		else if (point.index == 477) { cp477 = point.position; }
		else if (point.index == 478) { cp478 = point.position; }
		else if (point.index == 479) { cp479 = point.position; }
		else if (point.index == 480) { cp480 = point.position; }
		else if (point.index == 481) { cp481 = point.position; }
		else if (point.index == 482) { cp482 = point.position; }
		else if (point.index == 483) { cp483 = point.position; }
		else if (point.index == 484) { cp484 = point.position; }
		else if (point.index == 485) { cp485 = point.position; }
		else if (point.index == 486) { cp486 = point.position; }
		else if (point.index == 487) { cp487 = point.position; }
		else if (point.index == 488) { cp488 = point.position; }
		else if (point.index == 489) { cp489 = point.position; }
		else if (point.index == 490) { cp490 = point.position; }
		else if (point.index == 491) { cp491 = point.position; }
		else if (point.index == 492) { cp492 = point.position; }
		else if (point.index == 493) { cp493 = point.position; }
		else if (point.index == 494) { cp494 = point.position; }
		else if (point.index == 495) { cp495 = point.position; }
		else if (point.index == 496) { cp496 = point.position; }
		else if (point.index == 497) { cp497 = point.position; }
		else if (point.index == 498) { cp498 = point.position; }
		else if (point.index == 499) { cp499 = point.position; }
		else if (point.index == 500) { cp500 = point.position; }
		else if (point.index == 501) { cp501 = point.position; }
		else if (point.index == 502) { cp502 = point.position; }
		else if (point.index == 503) { cp503 = point.position; }
		else if (point.index == 504) { cp504 = point.position; }
		else if (point.index == 505) { cp505 = point.position; }
		else if (point.index == 506) { cp506 = point.position; }
		else if (point.index == 507) { cp507 = point.position; }
		else if (point.index == 508) { cp508 = point.position; }
		else if (point.index == 509) { cp509 = point.position; }
		else if (point.index == 510) { cp510 = point.position; }
		else if (point.index == 511) { cp511 = point.position; }
		else { Debug.Log("Control Point not found"); }
		// Debug.Log("Control Point Set");
	}

	public Vector3 GetPoint(Point point) {
		if (point.index == 0) { return cp; }
		else if (point.index == 1) { return cp1; }
		else if (point.index == 2) { return cp2; }
		else if (point.index == 3) { return cp3; }
		else if (point.index == 4) { return cp4; }
		else if (point.index == 5) { return cp5; }
		else if (point.index == 6) { return cp6; }
		else if (point.index == 7) { return cp7; }
		else if (point.index == 8) { return cp8; }
		else if (point.index == 9) { return cp9; }
		else if (point.index == 10) { return cp10; }
		else if (point.index == 11) { return cp11; }
		else if (point.index == 12) { return cp12; }
		else if (point.index == 13) { return cp13; }
		else if (point.index == 14) { return cp14; }
		else if (point.index == 15) { return cp15; }
		else if (point.index == 16) { return cp16; }
		else if (point.index == 17) { return cp17; }
		else if (point.index == 18) { return cp18; }
		else if (point.index == 19) { return cp19; }
		else if (point.index == 20) { return cp20; }
		else if (point.index == 21) { return cp21; }
		else if (point.index == 22) { return cp22; }
		else if (point.index == 23) { return cp23; }
		else if (point.index == 24) { return cp24; }
		else if (point.index == 25) { return cp25; }
		else if (point.index == 26) { return cp26; }
		else if (point.index == 27) { return cp27; }
		else if (point.index == 28) { return cp28; }
		else if (point.index == 29) { return cp29; }
		else if (point.index == 30) { return cp30; }
		else if (point.index == 31) { return cp31; }
		else if (point.index == 32) { return cp32; }
		else if (point.index == 33) { return cp33; }
		else if (point.index == 34) { return cp34; }
		else if (point.index == 35) { return cp35; }
		else if (point.index == 36) { return cp36; }
		else if (point.index == 37) { return cp37; }
		else if (point.index == 38) { return cp38; }
		else if (point.index == 39) { return cp39; }
		else if (point.index == 40) { return cp40; }
		else if (point.index == 41) { return cp41; }
		else if (point.index == 42) { return cp42; }
		else if (point.index == 43) { return cp43; }
		else if (point.index == 44) { return cp44; }
		else if (point.index == 45) { return cp45; }
		else if (point.index == 46) { return cp46; }
		else if (point.index == 47) { return cp47; }
		else if (point.index == 48) { return cp48; }
		else if (point.index == 49) { return cp49; }
		else if (point.index == 50) { return cp50; }
		else if (point.index == 51) { return cp51; }
		else if (point.index == 52) { return cp52; }
		else if (point.index == 53) { return cp53; }
		else if (point.index == 54) { return cp54; }
		else if (point.index == 55) { return cp55; }
		else if (point.index == 56) { return cp56; }
		else if (point.index == 57) { return cp57; }
		else if (point.index == 58) { return cp58; }
		else if (point.index == 59) { return cp59; }
		else if (point.index == 60) { return cp60; }
		else if (point.index == 61) { return cp61; }
		else if (point.index == 62) { return cp62; }
		else if (point.index == 63) { return cp63; }
		else if (point.index == 64) { return cp64; }
		else if (point.index == 65) { return cp65; }
		else if (point.index == 66) { return cp66; }
		else if (point.index == 67) { return cp67; }
		else if (point.index == 68) { return cp68; }
		else if (point.index == 69) { return cp69; }
		else if (point.index == 70) { return cp70; }
		else if (point.index == 71) { return cp71; }
		else if (point.index == 72) { return cp72; }
		else if (point.index == 73) { return cp73; }
		else if (point.index == 74) { return cp74; }
		else if (point.index == 75) { return cp75; }
		else if (point.index == 76) { return cp76; }
		else if (point.index == 77) { return cp77; }
		else if (point.index == 78) { return cp78; }
		else if (point.index == 79) { return cp79; }
		else if (point.index == 80) { return cp80; }
		else if (point.index == 81) { return cp81; }
		else if (point.index == 82) { return cp82; }
		else if (point.index == 83) { return cp83; }
		else if (point.index == 84) { return cp84; }
		else if (point.index == 85) { return cp85; }
		else if (point.index == 86) { return cp86; }
		else if (point.index == 87) { return cp87; }
		else if (point.index == 88) { return cp88; }
		else if (point.index == 89) { return cp89; }
		else if (point.index == 90) { return cp90; }
		else if (point.index == 91) { return cp91; }
		else if (point.index == 92) { return cp92; }
		else if (point.index == 93) { return cp93; }
		else if (point.index == 94) { return cp94; }
		else if (point.index == 95) { return cp95; }
		else if (point.index == 96) { return cp96; }
		else if (point.index == 97) { return cp97; }
		else if (point.index == 98) { return cp98; }
		else if (point.index == 99) { return cp99; }
		else if (point.index == 100) { return cp100; }
		else if (point.index == 101) { return cp101; }
		else if (point.index == 102) { return cp102; }
		else if (point.index == 103) { return cp103; }
		else if (point.index == 104) { return cp104; }
		else if (point.index == 105) { return cp105; }
		else if (point.index == 106) { return cp106; }
		else if (point.index == 107) { return cp107; }
		else if (point.index == 108) { return cp108; }
		else if (point.index == 109) { return cp109; }
		else if (point.index == 110) { return cp110; }
		else if (point.index == 111) { return cp111; }
		else if (point.index == 112) { return cp112; }
		else if (point.index == 113) { return cp113; }
		else if (point.index == 114) { return cp114; }
		else if (point.index == 115) { return cp115; }
		else if (point.index == 116) { return cp116; }
		else if (point.index == 117) { return cp117; }
		else if (point.index == 118) { return cp118; }
		else if (point.index == 119) { return cp119; }
		else if (point.index == 120) { return cp120; }
		else if (point.index == 121) { return cp121; }
		else if (point.index == 122) { return cp122; }
		else if (point.index == 123) { return cp123; }
		else if (point.index == 124) { return cp124; }
		else if (point.index == 125) { return cp125; }
		else if (point.index == 126) { return cp126; }
		else if (point.index == 127) { return cp127; }
		else if (point.index == 128) { return cp128; }
		else if (point.index == 129) { return cp129; }
		else if (point.index == 130) { return cp130; }
		else if (point.index == 131) { return cp131; }
		else if (point.index == 132) { return cp132; }
		else if (point.index == 133) { return cp133; }
		else if (point.index == 134) { return cp134; }
		else if (point.index == 135) { return cp135; }
		else if (point.index == 136) { return cp136; }
		else if (point.index == 137) { return cp137; }
		else if (point.index == 138) { return cp138; }
		else if (point.index == 139) { return cp139; }
		else if (point.index == 140) { return cp140; }
		else if (point.index == 141) { return cp141; }
		else if (point.index == 142) { return cp142; }
		else if (point.index == 143) { return cp143; }
		else if (point.index == 144) { return cp144; }
		else if (point.index == 145) { return cp145; }
		else if (point.index == 146) { return cp146; }
		else if (point.index == 147) { return cp147; }
		else if (point.index == 148) { return cp148; }
		else if (point.index == 149) { return cp149; }
		else if (point.index == 150) { return cp150; }
		else if (point.index == 151) { return cp151; }
		else if (point.index == 152) { return cp152; }
		else if (point.index == 153) { return cp153; }
		else if (point.index == 154) { return cp154; }
		else if (point.index == 155) { return cp155; }
		else if (point.index == 156) { return cp156; }
		else if (point.index == 157) { return cp157; }
		else if (point.index == 158) { return cp158; }
		else if (point.index == 159) { return cp159; }
		else if (point.index == 160) { return cp160; }
		else if (point.index == 161) { return cp161; }
		else if (point.index == 162) { return cp162; }
		else if (point.index == 163) { return cp163; }
		else if (point.index == 164) { return cp164; }
		else if (point.index == 165) { return cp165; }
		else if (point.index == 166) { return cp166; }
		else if (point.index == 167) { return cp167; }
		else if (point.index == 168) { return cp168; }
		else if (point.index == 169) { return cp169; }
		else if (point.index == 170) { return cp170; }
		else if (point.index == 171) { return cp171; }
		else if (point.index == 172) { return cp172; }
		else if (point.index == 173) { return cp173; }
		else if (point.index == 174) { return cp174; }
		else if (point.index == 175) { return cp175; }
		else if (point.index == 176) { return cp176; }
		else if (point.index == 177) { return cp177; }
		else if (point.index == 178) { return cp178; }
		else if (point.index == 179) { return cp179; }
		else if (point.index == 180) { return cp180; }
		else if (point.index == 181) { return cp181; }
		else if (point.index == 182) { return cp182; }
		else if (point.index == 183) { return cp183; }
		else if (point.index == 184) { return cp184; }
		else if (point.index == 185) { return cp185; }
		else if (point.index == 186) { return cp186; }
		else if (point.index == 187) { return cp187; }
		else if (point.index == 188) { return cp188; }
		else if (point.index == 189) { return cp189; }
		else if (point.index == 190) { return cp190; }
		else if (point.index == 191) { return cp191; }
		else if (point.index == 192) { return cp192; }
		else if (point.index == 193) { return cp193; }
		else if (point.index == 194) { return cp194; }
		else if (point.index == 195) { return cp195; }
		else if (point.index == 196) { return cp196; }
		else if (point.index == 197) { return cp197; }
		else if (point.index == 198) { return cp198; }
		else if (point.index == 199) { return cp199; }
		else if (point.index == 200) { return cp200; }
		else if (point.index == 201) { return cp201; }
		else if (point.index == 202) { return cp202; }
		else if (point.index == 203) { return cp203; }
		else if (point.index == 204) { return cp204; }
		else if (point.index == 205) { return cp205; }
		else if (point.index == 206) { return cp206; }
		else if (point.index == 207) { return cp207; }
		else if (point.index == 208) { return cp208; }
		else if (point.index == 209) { return cp209; }
		else if (point.index == 210) { return cp210; }
		else if (point.index == 211) { return cp211; }
		else if (point.index == 212) { return cp212; }
		else if (point.index == 213) { return cp213; }
		else if (point.index == 214) { return cp214; }
		else if (point.index == 215) { return cp215; }
		else if (point.index == 216) { return cp216; }
		else if (point.index == 217) { return cp217; }
		else if (point.index == 218) { return cp218; }
		else if (point.index == 219) { return cp219; }
		else if (point.index == 220) { return cp220; }
		else if (point.index == 221) { return cp221; }
		else if (point.index == 222) { return cp222; }
		else if (point.index == 223) { return cp223; }
		else if (point.index == 224) { return cp224; }
		else if (point.index == 225) { return cp225; }
		else if (point.index == 226) { return cp226; }
		else if (point.index == 227) { return cp227; }
		else if (point.index == 228) { return cp228; }
		else if (point.index == 229) { return cp229; }
		else if (point.index == 230) { return cp230; }
		else if (point.index == 231) { return cp231; }
		else if (point.index == 232) { return cp232; }
		else if (point.index == 233) { return cp233; }
		else if (point.index == 234) { return cp234; }
		else if (point.index == 235) { return cp235; }
		else if (point.index == 236) { return cp236; }
		else if (point.index == 237) { return cp237; }
		else if (point.index == 238) { return cp238; }
		else if (point.index == 239) { return cp239; }
		else if (point.index == 240) { return cp240; }
		else if (point.index == 241) { return cp241; }
		else if (point.index == 242) { return cp242; }
		else if (point.index == 243) { return cp243; }
		else if (point.index == 244) { return cp244; }
		else if (point.index == 245) { return cp245; }
		else if (point.index == 246) { return cp246; }
		else if (point.index == 247) { return cp247; }
		else if (point.index == 248) { return cp248; }
		else if (point.index == 249) { return cp249; }
		else if (point.index == 250) { return cp250; }
		else if (point.index == 251) { return cp251; }
		else if (point.index == 252) { return cp252; }
		else if (point.index == 253) { return cp253; }
		else if (point.index == 254) { return cp254; }
		else if (point.index == 255) { return cp255; }
		else if (point.index == 257) { return cp257; }
		else if (point.index == 258) { return cp258; }
		else if (point.index == 259) { return cp259; }
		else if (point.index == 260) { return cp260; }
		else if (point.index == 261) { return cp261; }
		else if (point.index == 262) { return cp262; }
		else if (point.index == 263) { return cp263; }
		else if (point.index == 264) { return cp264; }
		else if (point.index == 265) { return cp265; }
		else if (point.index == 266) { return cp266; }
		else if (point.index == 267) { return cp267; }
		else if (point.index == 268) { return cp268; }
		else if (point.index == 269) { return cp269; }
		else if (point.index == 270) { return cp270; }
		else if (point.index == 271) { return cp271; }
		else if (point.index == 272) { return cp272; }
		else if (point.index == 273) { return cp273; }
		else if (point.index == 274) { return cp274; }
		else if (point.index == 275) { return cp275; }
		else if (point.index == 276) { return cp276; }
		else if (point.index == 277) { return cp277; }
		else if (point.index == 278) { return cp278; }
		else if (point.index == 279) { return cp279; }
		else if (point.index == 280) { return cp280; }
		else if (point.index == 281) { return cp281; }
		else if (point.index == 282) { return cp282; }
		else if (point.index == 283) { return cp283; }
		else if (point.index == 284) { return cp284; }
		else if (point.index == 285) { return cp285; }
		else if (point.index == 286) { return cp286; }
		else if (point.index == 287) { return cp287; }
		else if (point.index == 288) { return cp288; }
		else if (point.index == 289) { return cp289; }
		else if (point.index == 290) { return cp290; }
		else if (point.index == 291) { return cp291; }
		else if (point.index == 292) { return cp292; }
		else if (point.index == 293) { return cp293; }
		else if (point.index == 294) { return cp294; }
		else if (point.index == 295) { return cp295; }
		else if (point.index == 296) { return cp296; }
		else if (point.index == 297) { return cp297; }
		else if (point.index == 298) { return cp298; }
		else if (point.index == 299) { return cp299; }
		else if (point.index == 300) { return cp300; }
		else if (point.index == 301) { return cp301; }
		else if (point.index == 302) { return cp302; }
		else if (point.index == 303) { return cp303; }
		else if (point.index == 304) { return cp304; }
		else if (point.index == 305) { return cp305; }
		else if (point.index == 306) { return cp306; }
		else if (point.index == 307) { return cp307; }
		else if (point.index == 308) { return cp308; }
		else if (point.index == 309) { return cp309; }
		else if (point.index == 310) { return cp310; }
		else if (point.index == 311) { return cp311; }
		else if (point.index == 312) { return cp312; }
		else if (point.index == 313) { return cp313; }
		else if (point.index == 314) { return cp314; }
		else if (point.index == 315) { return cp315; }
		else if (point.index == 316) { return cp316; }
		else if (point.index == 317) { return cp317; }
		else if (point.index == 318) { return cp318; }
		else if (point.index == 319) { return cp319; }
		else if (point.index == 320) { return cp320; }
		else if (point.index == 321) { return cp321; }
		else if (point.index == 322) { return cp322; }
		else if (point.index == 323) { return cp323; }
		else if (point.index == 324) { return cp324; }
		else if (point.index == 325) { return cp325; }
		else if (point.index == 326) { return cp326; }
		else if (point.index == 327) { return cp327; }
		else if (point.index == 328) { return cp328; }
		else if (point.index == 329) { return cp329; }
		else if (point.index == 330) { return cp330; }
		else if (point.index == 331) { return cp331; }
		else if (point.index == 332) { return cp332; }
		else if (point.index == 333) { return cp333; }
		else if (point.index == 334) { return cp334; }
		else if (point.index == 335) { return cp335; }
		else if (point.index == 336) { return cp336; }
		else if (point.index == 337) { return cp337; }
		else if (point.index == 338) { return cp338; }
		else if (point.index == 339) { return cp339; }
		else if (point.index == 340) { return cp340; }
		else if (point.index == 341) { return cp341; }
		else if (point.index == 342) { return cp342; }
		else if (point.index == 343) { return cp343; }
		else if (point.index == 344) { return cp344; }
		else if (point.index == 345) { return cp345; }
		else if (point.index == 346) { return cp346; }
		else if (point.index == 347) { return cp347; }
		else if (point.index == 348) { return cp348; }
		else if (point.index == 349) { return cp349; }
		else if (point.index == 350) { return cp350; }
		else if (point.index == 351) { return cp351; }
		else if (point.index == 352) { return cp352; }
		else if (point.index == 353) { return cp353; }
		else if (point.index == 354) { return cp354; }
		else if (point.index == 355) { return cp355; }
		else if (point.index == 356) { return cp356; }
		else if (point.index == 357) { return cp357; }
		else if (point.index == 358) { return cp358; }
		else if (point.index == 359) { return cp359; }
		else if (point.index == 360) { return cp360; }
		else if (point.index == 361) { return cp361; }
		else if (point.index == 362) { return cp362; }
		else if (point.index == 363) { return cp363; }
		else if (point.index == 364) { return cp364; }
		else if (point.index == 365) { return cp365; }
		else if (point.index == 366) { return cp366; }
		else if (point.index == 367) { return cp367; }
		else if (point.index == 368) { return cp368; }
		else if (point.index == 369) { return cp369; }
		else if (point.index == 370) { return cp370; }
		else if (point.index == 371) { return cp371; }
		else if (point.index == 372) { return cp372; }
		else if (point.index == 373) { return cp373; }
		else if (point.index == 374) { return cp374; }
		else if (point.index == 375) { return cp375; }
		else if (point.index == 376) { return cp376; }
		else if (point.index == 377) { return cp377; }
		else if (point.index == 378) { return cp378; }
		else if (point.index == 379) { return cp379; }
		else if (point.index == 380) { return cp380; }
		else if (point.index == 381) { return cp381; }
		else if (point.index == 382) { return cp382; }
		else if (point.index == 383) { return cp383; }
		else if (point.index == 384) { return cp384; }
		else if (point.index == 385) { return cp385; }
		else if (point.index == 386) { return cp386; }
		else if (point.index == 387) { return cp387; }
		else if (point.index == 388) { return cp388; }
		else if (point.index == 389) { return cp389; }
		else if (point.index == 390) { return cp390; }
		else if (point.index == 391) { return cp391; }
		else if (point.index == 392) { return cp392; }
		else if (point.index == 393) { return cp393; }
		else if (point.index == 394) { return cp394; }
		else if (point.index == 395) { return cp395; }
		else if (point.index == 396) { return cp396; }
		else if (point.index == 397) { return cp397; }
		else if (point.index == 398) { return cp398; }
		else if (point.index == 399) { return cp399; }
		else if (point.index == 400) { return cp400; }
		else if (point.index == 401) { return cp401; }
		else if (point.index == 402) { return cp402; }
		else if (point.index == 403) { return cp403; }
		else if (point.index == 404) { return cp404; }
		else if (point.index == 405) { return cp405; }
		else if (point.index == 406) { return cp406; }
		else if (point.index == 407) { return cp407; }
		else if (point.index == 408) { return cp408; }
		else if (point.index == 409) { return cp409; }
		else if (point.index == 410) { return cp410; }
		else if (point.index == 411) { return cp411; }
		else if (point.index == 412) { return cp412; }
		else if (point.index == 413) { return cp413; }
		else if (point.index == 414) { return cp414; }
		else if (point.index == 415) { return cp415; }
		else if (point.index == 416) { return cp416; }
		else if (point.index == 417) { return cp417; }
		else if (point.index == 418) { return cp418; }
		else if (point.index == 419) { return cp419; }
		else if (point.index == 420) { return cp420; }
		else if (point.index == 421) { return cp421; }
		else if (point.index == 422) { return cp422; }
		else if (point.index == 423) { return cp423; }
		else if (point.index == 424) { return cp424; }
		else if (point.index == 425) { return cp425; }
		else if (point.index == 426) { return cp426; }
		else if (point.index == 427) { return cp427; }
		else if (point.index == 428) { return cp428; }
		else if (point.index == 429) { return cp429; }
		else if (point.index == 430) { return cp430; }
		else if (point.index == 431) { return cp431; }
		else if (point.index == 432) { return cp432; }
		else if (point.index == 433) { return cp433; }
		else if (point.index == 434) { return cp434; }
		else if (point.index == 435) { return cp435; }
		else if (point.index == 436) { return cp436; }
		else if (point.index == 437) { return cp437; }
		else if (point.index == 438) { return cp438; }
		else if (point.index == 439) { return cp439; }
		else if (point.index == 440) { return cp440; }
		else if (point.index == 441) { return cp441; }
		else if (point.index == 442) { return cp442; }
		else if (point.index == 443) { return cp443; }
		else if (point.index == 444) { return cp444; }
		else if (point.index == 445) { return cp445; }
		else if (point.index == 446) { return cp446; }
		else if (point.index == 447) { return cp447; }
		else if (point.index == 448) { return cp448; }
		else if (point.index == 449) { return cp449; }
		else if (point.index == 450) { return cp450; }
		else if (point.index == 451) { return cp451; }
		else if (point.index == 452) { return cp452; }
		else if (point.index == 453) { return cp453; }
		else if (point.index == 454) { return cp454; }
		else if (point.index == 455) { return cp455; }
		else if (point.index == 456) { return cp456; }
		else if (point.index == 457) { return cp457; }
		else if (point.index == 458) { return cp458; }
		else if (point.index == 459) { return cp459; }
		else if (point.index == 460) { return cp460; }
		else if (point.index == 461) { return cp461; }
		else if (point.index == 462) { return cp462; }
		else if (point.index == 463) { return cp463; }
		else if (point.index == 464) { return cp464; }
		else if (point.index == 465) { return cp465; }
		else if (point.index == 466) { return cp466; }
		else if (point.index == 467) { return cp467; }
		else if (point.index == 468) { return cp468; }
		else if (point.index == 469) { return cp469; }
		else if (point.index == 470) { return cp470; }
		else if (point.index == 471) { return cp471; }
		else if (point.index == 472) { return cp472; }
		else if (point.index == 473) { return cp473; }
		else if (point.index == 474) { return cp474; }
		else if (point.index == 475) { return cp475; }
		else if (point.index == 476) { return cp476; }
		else if (point.index == 477) { return cp477; }
		else if (point.index == 478) { return cp478; }
		else if (point.index == 479) { return cp479; }
		else if (point.index == 480) { return cp480; }
		else if (point.index == 481) { return cp481; }
		else if (point.index == 482) { return cp482; }
		else if (point.index == 483) { return cp483; }
		else if (point.index == 484) { return cp484; }
		else if (point.index == 485) { return cp485; }
		else if (point.index == 486) { return cp486; }
		else if (point.index == 487) { return cp487; }
		else if (point.index == 488) { return cp488; }
		else if (point.index == 489) { return cp489; }
		else if (point.index == 490) { return cp490; }
		else if (point.index == 491) { return cp491; }
		else if (point.index == 492) { return cp492; }
		else if (point.index == 493) { return cp493; }
		else if (point.index == 494) { return cp494; }
		else if (point.index == 495) { return cp495; }
		else if (point.index == 496) { return cp496; }
		else if (point.index == 497) { return cp497; }
		else if (point.index == 498) { return cp498; }
		else if (point.index == 499) { return cp499; }
		else if (point.index == 500) { return cp500; }
		else if (point.index == 501) { return cp501; }
		else if (point.index == 502) { return cp502; }
		else if (point.index == 503) { return cp503; }
		else if (point.index == 504) { return cp504; }
		else if (point.index == 505) { return cp505; }
		else if (point.index == 506) { return cp506; }
		else if (point.index == 507) { return cp507; }
		else if (point.index == 508) { return cp508; }
		else if (point.index == 509) { return cp509; }
		else if (point.index == 510) { return cp510; }
		else if (point.index == 511) { return cp511; }
		else { return Vector3.zero; }
	}

	[System.Serializable]
    public class Point {
        public Vector3 position = Vector3.zero;
		public Vector3 originalPosition = Vector3.zero;
        public int index = -1;
        public Point(Vector3 pos) { 
			position = pos;
			originalPosition = pos;
		}

		public void ResetPosition() {
			position = originalPosition;
		}
    }
}