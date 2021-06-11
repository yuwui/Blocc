// FastNoise.cs
//
// MIT License
//
// Copyright(c) 2017 Jordan Peck
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// The developer's email is jorzixdan.me2@gzixmail.com (for great email, take
// off every 'zix'.)
//

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Blocc.Engine.Algorithm.FastNoise
{
    public partial class FastNoise
    {
        private const int CellularIndexMax = 3;
        private const int PrimeX = 1619;
        private const int PrimeY = 31337;
        private const int PrimeZ = 6971;
        private const int PrimeW = 1013;
        private const float F3 = 1f / 3f;
        private const float G3 = 1f / 6f;
        private const float G33 = G3 * 3 - 1;
        private const float Sqrt3 = 1.7320508075688772935274463415059f;
        private const float F2 = 0.5f * (Sqrt3 - 1f);
        private const float G2 = (3f - Sqrt3) / 6f;
        private const float F4 = (2.23606797f - 1f) / 4f;
        private const float G4 = (5f - 2.23606797f) / 20f;
        private const float Cubic3DBounding = 1f / (1.5f * 1.5f * 1.5f);
        private const float Cubic2DBounding = 1f / (1.5f * 1.5f);

        private static ReadOnlySpan<Vector2> Grad2D => new[]
        {
            new Vector2(-1,-1), new Vector2( 1,-1), new Vector2(-1, 1), new Vector2( 1, 1),
            new Vector2( 0,-1), new Vector2(-1, 0), new Vector2( 0, 1), new Vector2( 1, 0),
        };

        private static ReadOnlySpan<Vector3> Grad3D => new[]
        {
            new Vector3( 1, 1, 0), new Vector3(-1, 1, 0), new Vector3( 1,-1, 0), new Vector3(-1,-1, 0),
            new Vector3( 1, 0, 1), new Vector3(-1, 0, 1), new Vector3( 1, 0,-1), new Vector3(-1, 0,-1),
            new Vector3( 0, 1, 1), new Vector3( 0,-1, 1), new Vector3( 0, 1,-1), new Vector3( 0,-1,-1),
            new Vector3( 1, 1, 0), new Vector3( 0,-1, 1), new Vector3(-1, 1, 0), new Vector3( 0,-1,-1),
        };

        private static ReadOnlySpan<Vector2> Cell2D => new[]
        {
            new Vector2(-0.2700222198f, -0.9628540911f), new Vector2(0.3863092627f, -0.9223693152f), new Vector2(0.04444859006f, -0.999011673f), new Vector2(-0.5992523158f, -0.8005602176f), new Vector2(-0.7819280288f, 0.6233687174f), new Vector2(0.9464672271f, 0.3227999196f), new Vector2(-0.6514146797f, -0.7587218957f), new Vector2(0.9378472289f, 0.347048376f),
            new Vector2(-0.8497875957f, -0.5271252623f), new Vector2(-0.879042592f, 0.4767432447f), new Vector2(-0.892300288f, -0.4514423508f), new Vector2(-0.379844434f, -0.9250503802f), new Vector2(-0.9951650832f, 0.0982163789f), new Vector2(0.7724397808f, -0.6350880136f), new Vector2(0.7573283322f, -0.6530343002f), new Vector2(-0.9928004525f, -0.119780055f),
            new Vector2(-0.0532665713f, 0.9985803285f), new Vector2(0.9754253726f, -0.2203300762f), new Vector2(-0.7665018163f, 0.6422421394f), new Vector2(0.991636706f, 0.1290606184f), new Vector2(-0.994696838f, 0.1028503788f), new Vector2(-0.5379205513f, -0.84299554f), new Vector2(0.5022815471f, -0.8647041387f), new Vector2(0.4559821461f, -0.8899889226f),
            new Vector2(-0.8659131224f, -0.5001944266f), new Vector2(0.0879458407f, -0.9961252577f), new Vector2(-0.5051684983f, 0.8630207346f), new Vector2(0.7753185226f, -0.6315704146f), new Vector2(-0.6921944612f, 0.7217110418f), new Vector2(-0.5191659449f, -0.8546734591f), new Vector2(0.8978622882f, -0.4402764035f), new Vector2(-0.1706774107f, 0.9853269617f),
            new Vector2(-0.9353430106f, -0.3537420705f), new Vector2(-0.9992404798f, 0.03896746794f), new Vector2(-0.2882064021f, -0.9575683108f), new Vector2(-0.9663811329f, 0.2571137995f), new Vector2(-0.8759714238f, -0.4823630009f), new Vector2(-0.8303123018f, -0.5572983775f), new Vector2(0.05110133755f, -0.9986934731f), new Vector2(-0.8558373281f, -0.5172450752f),
            new Vector2(0.09887025282f, 0.9951003332f), new Vector2(0.9189016087f, 0.3944867976f), new Vector2(-0.2439375892f, -0.9697909324f), new Vector2(-0.8121409387f, -0.5834613061f), new Vector2(-0.9910431363f, 0.1335421355f), new Vector2(0.8492423985f, -0.5280031709f), new Vector2(-0.9717838994f, -0.2358729591f), new Vector2(0.9949457207f, 0.1004142068f),
            new Vector2(0.6241065508f, -0.7813392434f), new Vector2(0.662910307f, 0.7486988212f), new Vector2(-0.7197418176f, 0.6942418282f), new Vector2(-0.8143370775f, -0.5803922158f), new Vector2(0.104521054f, -0.9945226741f), new Vector2(-0.1065926113f, -0.9943027784f), new Vector2(0.445799684f, -0.8951327509f), new Vector2(0.105547406f, 0.9944142724f),
            new Vector2(-0.992790267f, 0.1198644477f), new Vector2(-0.8334366408f, 0.552615025f), new Vector2(0.9115561563f, -0.4111755999f), new Vector2(0.8285544909f, -0.5599084351f), new Vector2(0.7217097654f, -0.6921957921f), new Vector2(0.4940492677f, -0.8694339084f), new Vector2(-0.3652321272f, -0.9309164803f), new Vector2(-0.9696606758f, 0.2444548501f),
            new Vector2(0.08925509731f, -0.996008799f), new Vector2(0.5354071276f, -0.8445941083f), new Vector2(-0.1053576186f, 0.9944343981f), new Vector2(-0.9890284586f, 0.1477251101f), new Vector2(0.004856104961f, 0.9999882091f), new Vector2(0.9885598478f, 0.1508291331f), new Vector2(0.9286129562f, -0.3710498316f), new Vector2(-0.5832393863f, -0.8123003252f),
            new Vector2(0.3015207509f, 0.9534596146f), new Vector2(-0.9575110528f, 0.2883965738f), new Vector2(0.9715802154f, -0.2367105511f), new Vector2(0.229981792f, 0.9731949318f), new Vector2(0.955763816f, -0.2941352207f), new Vector2(0.740956116f, 0.6715534485f), new Vector2(-0.9971513787f, -0.07542630764f), new Vector2(0.6905710663f, -0.7232645452f),
            new Vector2(-0.290713703f, -0.9568100872f), new Vector2(0.5912777791f, -0.8064679708f), new Vector2(-0.9454592212f, -0.325740481f), new Vector2(0.6664455681f, 0.74555369f), new Vector2(0.6236134912f, 0.7817328275f), new Vector2(0.9126993851f, -0.4086316587f), new Vector2(-0.8191762011f, 0.5735419353f), new Vector2(-0.8812745759f, -0.4726046147f),
            new Vector2(0.9953313627f, 0.09651672651f), new Vector2(0.9855650846f, -0.1692969699f), new Vector2(-0.8495980887f, 0.5274306472f), new Vector2(0.6174853946f, -0.7865823463f), new Vector2(0.8508156371f, 0.52546432f), new Vector2(0.9985032451f, -0.05469249926f), new Vector2(0.1971371563f, -0.9803759185f), new Vector2(0.6607855748f, -0.7505747292f),
            new Vector2(-0.03097494063f, 0.9995201614f), new Vector2(-0.6731660801f, 0.739491331f), new Vector2(-0.7195018362f, -0.6944905383f), new Vector2(0.9727511689f, 0.2318515979f), new Vector2(0.9997059088f, -0.0242506907f), new Vector2(0.4421787429f, -0.8969269532f), new Vector2(0.9981350961f, -0.061043673f), new Vector2(-0.9173660799f, -0.3980445648f),
            new Vector2(-0.8150056635f, -0.5794529907f), new Vector2(-0.8789331304f, 0.4769450202f), new Vector2(0.0158605829f, 0.999874213f), new Vector2(-0.8095464474f, 0.5870558317f), new Vector2(-0.9165898907f, -0.3998286786f), new Vector2(-0.8023542565f, 0.5968480938f), new Vector2(-0.5176737917f, 0.8555780767f), new Vector2(-0.8154407307f, -0.5788405779f),
            new Vector2(0.4022010347f, -0.9155513791f), new Vector2(-0.9052556868f, -0.4248672045f), new Vector2(0.7317445619f, 0.6815789728f), new Vector2(-0.5647632201f, -0.8252529947f), new Vector2(-0.8403276335f, -0.5420788397f), new Vector2(-0.9314281527f, 0.363925262f), new Vector2(0.5238198472f, 0.8518290719f), new Vector2(0.7432803869f, -0.6689800195f),
            new Vector2(-0.985371561f, -0.1704197369f), new Vector2(0.4601468731f, 0.88784281f), new Vector2(0.825855404f, 0.5638819483f), new Vector2(0.6182366099f, 0.7859920446f), new Vector2(0.8331502863f, -0.553046653f), new Vector2(0.1500307506f, 0.9886813308f), new Vector2(-0.662330369f, -0.7492119075f), new Vector2(-0.668598664f, 0.743623444f),
            new Vector2(0.7025606278f, 0.7116238924f), new Vector2(-0.5419389763f, -0.8404178401f), new Vector2(-0.3388616456f, 0.9408362159f), new Vector2(0.8331530315f, 0.5530425174f), new Vector2(-0.2989720662f, -0.9542618632f), new Vector2(0.2638522993f, 0.9645630949f), new Vector2(0.124108739f, -0.9922686234f), new Vector2(-0.7282649308f, -0.6852956957f),
            new Vector2(0.6962500149f, 0.7177993569f), new Vector2(-0.9183535368f, 0.3957610156f), new Vector2(-0.6326102274f, -0.7744703352f), new Vector2(-0.9331891859f, -0.359385508f), new Vector2(-0.1153779357f, -0.9933216659f), new Vector2(0.9514974788f, -0.3076565421f), new Vector2(-0.08987977445f, -0.9959526224f), new Vector2(0.6678496916f, 0.7442961705f),
            new Vector2(0.7952400393f, -0.6062947138f), new Vector2(-0.6462007402f, -0.7631674805f), new Vector2(-0.2733598753f, 0.9619118351f), new Vector2(0.9669590226f, -0.254931851f), new Vector2(-0.9792894595f, 0.2024651934f), new Vector2(-0.5369502995f, -0.8436138784f), new Vector2(-0.270036471f, -0.9628500944f), new Vector2(-0.6400277131f, 0.7683518247f),
            new Vector2(-0.7854537493f, -0.6189203566f), new Vector2(0.06005905383f, -0.9981948257f), new Vector2(-0.02455770378f, 0.9996984141f), new Vector2(-0.65983623f, 0.751409442f), new Vector2(-0.6253894466f, -0.7803127835f), new Vector2(-0.6210408851f, -0.7837781695f), new Vector2(0.8348888491f, 0.5504185768f), new Vector2(-0.1592275245f, 0.9872419133f),
            new Vector2(0.8367622488f, 0.5475663786f), new Vector2(-0.8675753916f, -0.4973056806f), new Vector2(-0.2022662628f, -0.9793305667f), new Vector2(0.9399189937f, 0.3413975472f), new Vector2(0.9877404807f, -0.1561049093f), new Vector2(-0.9034455656f, 0.4287028224f), new Vector2(0.1269804218f, -0.9919052235f), new Vector2(-0.3819600854f, 0.924178821f),
            new Vector2(0.9754625894f, 0.2201652486f), new Vector2(-0.3204015856f, -0.9472818081f), new Vector2(-0.9874760884f, 0.1577687387f), new Vector2(0.02535348474f, -0.9996785487f), new Vector2(0.4835130794f, -0.8753371362f), new Vector2(-0.2850799925f, -0.9585037287f), new Vector2(-0.06805516006f, -0.99768156f), new Vector2(-0.7885244045f, -0.6150034663f),
            new Vector2(0.3185392127f, -0.9479096845f), new Vector2(0.8880043089f, 0.4598351306f), new Vector2(0.6476921488f, -0.7619021462f), new Vector2(0.9820241299f, 0.1887554194f), new Vector2(0.9357275128f, -0.3527237187f), new Vector2(-0.8894895414f, 0.4569555293f), new Vector2(0.7922791302f, 0.6101588153f), new Vector2(0.7483818261f, 0.6632681526f),
            new Vector2(-0.7288929755f, -0.6846276581f), new Vector2(0.8729032783f, -0.4878932944f), new Vector2(0.8288345784f, 0.5594937369f), new Vector2(0.08074567077f, 0.9967347374f), new Vector2(0.9799148216f, -0.1994165048f), new Vector2(-0.580730673f, -0.8140957471f), new Vector2(-0.4700049791f, -0.8826637636f), new Vector2(0.2409492979f, 0.9705377045f),
            new Vector2(0.9437816757f, -0.3305694308f), new Vector2(-0.8927998638f, -0.4504535528f), new Vector2(-0.8069622304f, 0.5906030467f), new Vector2(0.06258973166f, 0.9980393407f), new Vector2(-0.9312597469f, 0.3643559849f), new Vector2(0.5777449785f, 0.8162173362f), new Vector2(-0.3360095855f, -0.941858566f), new Vector2(0.697932075f, -0.7161639607f),
            new Vector2(-0.002008157227f, -0.9999979837f), new Vector2(-0.1827294312f, -0.9831632392f), new Vector2(-0.6523911722f, 0.7578824173f), new Vector2(-0.4302626911f, -0.9027037258f), new Vector2(-0.9985126289f, -0.05452091251f), new Vector2(-0.01028102172f, -0.9999471489f), new Vector2(-0.4946071129f, 0.8691166802f), new Vector2(-0.2999350194f, 0.9539596344f),
            new Vector2(0.8165471961f, 0.5772786819f), new Vector2(0.2697460475f, 0.962931498f), new Vector2(-0.7306287391f, -0.6827749597f), new Vector2(-0.7590952064f, -0.6509796216f), new Vector2(-0.907053853f, 0.4210146171f), new Vector2(-0.5104861064f, -0.8598860013f), new Vector2(0.8613350597f, 0.5080373165f), new Vector2(0.5007881595f, -0.8655698812f),
            new Vector2(-0.654158152f, 0.7563577938f), new Vector2(-0.8382755311f, -0.545246856f), new Vector2(0.6940070834f, 0.7199681717f), new Vector2(0.06950936031f, 0.9975812994f), new Vector2(0.1702942185f, -0.9853932612f), new Vector2(0.2695973274f, 0.9629731466f), new Vector2(0.5519612192f, -0.8338697815f), new Vector2(0.225657487f, -0.9742067022f),
            new Vector2(0.4215262855f, -0.9068161835f), new Vector2(0.4881873305f, -0.8727388672f), new Vector2(-0.3683854996f, -0.9296731273f), new Vector2(-0.9825390578f, 0.1860564427f), new Vector2(0.81256471f, 0.5828709909f), new Vector2(0.3196460933f, -0.9475370046f), new Vector2(0.9570913859f, 0.2897862643f), new Vector2(-0.6876655497f, -0.7260276109f),
            new Vector2(-0.9988770922f, -0.047376731f), new Vector2(-0.1250179027f, 0.992154486f), new Vector2(-0.8280133617f, 0.560708367f), new Vector2(0.9324863769f, -0.3612051451f), new Vector2(0.6394653183f, 0.7688199442f), new Vector2(-0.01623847064f, -0.9998681473f), new Vector2(-0.9955014666f, -0.09474613458f), new Vector2(-0.81453315f, 0.580117012f),
            new Vector2(0.4037327978f, -0.9148769469f), new Vector2(0.9944263371f, 0.1054336766f), new Vector2(-0.1624711654f, 0.9867132919f), new Vector2(-0.9949487814f, -0.100383875f), new Vector2(-0.6995302564f, 0.7146029809f), new Vector2(0.5263414922f, -0.85027327f), new Vector2(-0.5395221479f, 0.841971408f), new Vector2(0.6579370318f, 0.7530729462f),
            new Vector2(0.01426758847f, -0.9998982128f), new Vector2(-0.6734383991f, 0.7392433447f), new Vector2(0.639412098f, -0.7688642071f), new Vector2(0.9211571421f, 0.3891908523f), new Vector2(-0.146637214f, -0.9891903394f), new Vector2(-0.782318098f, 0.6228791163f), new Vector2(-0.5039610839f, -0.8637263605f), new Vector2(-0.7743120191f, -0.6328039957f),
        };

        private static ReadOnlySpan<Vector3> Cell3D => new[]
        {
            new Vector3(-0.7292736885f, -0.6618439697f, 0.1735581948f), new Vector3(0.790292081f, -0.5480887466f, -0.2739291014f), new Vector3(0.7217578935f, 0.6226212466f, -0.3023380997f), new Vector3(0.565683137f, -0.8208298145f, -0.0790000257f), new Vector3(0.760049034f, -0.5555979497f, -0.3370999617f), new Vector3(0.3713945616f, 0.5011264475f, 0.7816254623f), new Vector3(-0.1277062463f, -0.4254438999f, -0.8959289049f), new Vector3(-0.2881560924f, -0.5815838982f, 0.7607405838f),
            new Vector3(0.5849561111f, -0.662820239f, -0.4674352136f), new Vector3(0.3307171178f, 0.0391653737f, 0.94291689f), new Vector3(0.8712121778f, -0.4113374369f, -0.2679381538f), new Vector3(0.580981015f, 0.7021915846f, 0.4115677815f), new Vector3(0.503756873f, 0.6330056931f, -0.5878203852f), new Vector3(0.4493712205f, 0.601390195f, 0.6606022552f), new Vector3(-0.6878403724f, 0.09018890807f, -0.7202371714f), new Vector3(-0.5958956522f, -0.6469350577f, 0.475797649f),
            new Vector3(-0.5127052122f, 0.1946921978f, -0.8361987284f), new Vector3(-0.9911507142f, -0.05410276466f, -0.1212153153f), new Vector3(-0.2149721042f, 0.9720882117f, -0.09397607749f), new Vector3(-0.7518650936f, -0.5428057603f, 0.3742469607f), new Vector3(0.5237068895f, 0.8516377189f, -0.02107817834f), new Vector3(0.6333504779f, 0.1926167129f, -0.7495104896f), new Vector3(-0.06788241606f, 0.3998305789f, 0.9140719259f), new Vector3(-0.5538628599f, -0.4729896695f, -0.6852128902f),
            new Vector3(-0.7261455366f, -0.5911990757f, 0.3509933228f), new Vector3(-0.9229274737f, -0.1782808786f, 0.3412049336f), new Vector3(-0.6968815002f, 0.6511274338f, 0.3006480328f), new Vector3(0.9608044783f, -0.2098363234f, -0.1811724921f), new Vector3(0.06817146062f, -0.9743405129f, 0.2145069156f), new Vector3(-0.3577285196f, -0.6697087264f, -0.6507845481f), new Vector3(-0.1868621131f, 0.7648617052f, -0.6164974636f), new Vector3(-0.6541697588f, 0.3967914832f, 0.6439087246f),
            new Vector3(0.6993340405f, -0.6164538506f, 0.3618239211f), new Vector3(-0.1546665739f, 0.6291283928f, 0.7617583057f), new Vector3(-0.6841612949f, -0.2580482182f, -0.6821542638f), new Vector3(0.5383980957f, 0.4258654885f, 0.7271630328f), new Vector3(-0.5026987823f, -0.7939832935f, -0.3418836993f), new Vector3(0.3202971715f, 0.2834415347f, 0.9039195862f), new Vector3(0.8683227101f, -0.0003762656404f, -0.4959995258f), new Vector3(0.791120031f, -0.08511045745f, 0.6057105799f),
            new Vector3(-0.04011016052f, -0.4397248749f, 0.8972364289f), new Vector3(0.9145119872f, 0.3579346169f, -0.1885487608f), new Vector3(-0.9612039066f, -0.2756484276f, 0.01024666929f), new Vector3(0.6510361721f, -0.2877799159f, -0.7023778346f), new Vector3(-0.2041786351f, 0.7365237271f, 0.644859585f), new Vector3(-0.7718263711f, 0.3790626912f, 0.5104855816f), new Vector3(-0.3060082741f, -0.7692987727f, 0.5608371729f), new Vector3(0.454007341f, -0.5024843065f, 0.7357899537f),
            new Vector3(0.4816795475f, 0.6021208291f, -0.6367380315f), new Vector3(0.6961980369f, -0.3222197429f, 0.641469197f), new Vector3(-0.6532160499f, -0.6781148932f, 0.3368515753f), new Vector3(0.5089301236f, -0.6154662304f, -0.6018234363f), new Vector3(-0.1635919754f, -0.9133604627f, -0.372840892f), new Vector3(0.52408019f, -0.8437664109f, 0.1157505864f), new Vector3(0.5902587356f, 0.4983817807f, -0.6349883666f), new Vector3(0.5863227872f, 0.494764745f, 0.6414307729f),
            new Vector3(0.6779335087f, 0.2341345225f, 0.6968408593f), new Vector3(0.7177054546f, -0.6858979348f, 0.120178631f), new Vector3(-0.5328819713f, -0.5205125012f, 0.6671608058f), new Vector3(-0.8654874251f, -0.0700727088f, -0.4960053754f), new Vector3(-0.2861810166f, 0.7952089234f, 0.5345495242f), new Vector3(-0.04849529634f, 0.9810836427f, -0.1874115585f), new Vector3(-0.6358521667f, 0.6058348682f, 0.4781800233f), new Vector3(0.6254794696f, -0.2861619734f, 0.7258696564f),
            new Vector3(-0.2585259868f, 0.5061949264f, -0.8227581726f), new Vector3(0.02136306781f, 0.5064016808f, -0.8620330371f), new Vector3(0.200111773f, 0.8599263484f, 0.4695550591f), new Vector3(0.4743561372f, 0.6014985084f, -0.6427953014f), new Vector3(0.6622993731f, -0.5202474575f, -0.5391679918f), new Vector3(0.08084972818f, -0.6532720452f, 0.7527940996f), new Vector3(-0.6893687501f, 0.0592860349f, 0.7219805347f), new Vector3(-0.1121887082f, -0.9673185067f, 0.2273952515f),
            new Vector3(0.7344116094f, 0.5979668656f, -0.3210532909f), new Vector3(0.5789393465f, -0.2488849713f, 0.7764570201f), new Vector3(0.6988182827f, 0.3557169806f, -0.6205791146f), new Vector3(-0.8636845529f, -0.2748771249f, -0.4224826141f), new Vector3(-0.4247027957f, -0.4640880967f, 0.777335046f), new Vector3(0.5257722489f, -0.8427017621f, 0.1158329937f), new Vector3(0.9343830603f, 0.316302472f, -0.1639543925f), new Vector3(-0.1016836419f, -0.8057303073f, -0.5834887393f),
            new Vector3(-0.6529238969f, 0.50602126f, -0.5635892736f), new Vector3(-0.2465286165f, -0.9668205684f, -0.06694497494f), new Vector3(-0.9776897119f, -0.2099250524f, -0.007368825344f), new Vector3(0.7736893337f, 0.5734244712f, 0.2694238123f), new Vector3(-0.6095087895f, 0.4995678998f, 0.6155736747f), new Vector3(0.5794535482f, 0.7434546771f, 0.3339292269f), new Vector3(-0.8226211154f, 0.08142581855f, 0.5627293636f), new Vector3(-0.510385483f, 0.4703667658f, 0.7199039967f),
            new Vector3(-0.5764971849f, -0.07231656274f, -0.8138926898f), new Vector3(0.7250628871f, 0.3949971505f, -0.5641463116f), new Vector3(-0.1525424005f, 0.4860840828f, -0.8604958341f), new Vector3(-0.5550976208f, -0.4957820792f, 0.667882296f), new Vector3(-0.1883614327f, 0.9145869398f, 0.357841725f), new Vector3(0.7625556724f, -0.5414408243f, -0.3540489801f), new Vector3(-0.5870231946f, -0.3226498013f, -0.7424963803f), new Vector3(0.3051124198f, 0.2262544068f, -0.9250488391f),
            new Vector3(0.6379576059f, 0.577242424f, -0.5097070502f), new Vector3(-0.5966775796f, 0.1454852398f, -0.7891830656f), new Vector3(-0.658330573f, 0.6555487542f, -0.3699414651f), new Vector3(0.7434892426f, 0.2351084581f, 0.6260573129f), new Vector3(0.5562114096f, 0.8264360377f, -0.0873632843f), new Vector3(-0.3028940016f, -0.8251527185f, 0.4768419182f), new Vector3(0.1129343818f, -0.985888439f, -0.1235710781f), new Vector3(0.5937652891f, -0.5896813806f, 0.5474656618f),
            new Vector3(0.6757964092f, -0.5835758614f, -0.4502648413f), new Vector3(0.7242302609f, -0.1152719764f, 0.6798550586f), new Vector3(-0.9511914166f, 0.0753623979f, -0.2992580792f), new Vector3(0.2539470961f, -0.1886339355f, 0.9486454084f), new Vector3(0.571433621f, -0.1679450851f, -0.8032795685f), new Vector3(-0.06778234979f, 0.3978269256f, 0.9149531629f), new Vector3(0.6074972649f, 0.733060024f, -0.3058922593f), new Vector3(-0.5435478392f, 0.1675822484f, 0.8224791405f),
            new Vector3(-0.5876678086f, -0.3380045064f, -0.7351186982f), new Vector3(-0.7967562402f, 0.04097822706f, -0.6029098428f), new Vector3(-0.1996350917f, 0.8706294745f, 0.4496111079f), new Vector3(-0.02787660336f, -0.9106232682f, -0.4122962022f), new Vector3(-0.7797625996f, -0.6257634692f, 0.01975775581f), new Vector3(-0.5211232846f, 0.7401644346f, -0.4249554471f), new Vector3(0.8575424857f, 0.4053272873f, -0.3167501783f), new Vector3(0.1045223322f, 0.8390195772f, -0.5339674439f),
            new Vector3(0.3501822831f, 0.9242524096f, -0.1520850155f), new Vector3(0.1987849858f, 0.07647613266f, 0.9770547224f), new Vector3(0.7845996363f, 0.6066256811f, -0.1280964233f), new Vector3(0.09006737436f, -0.9750989929f, -0.2026569073f), new Vector3(-0.8274343547f, -0.542299559f, 0.1458203587f), new Vector3(-0.3485797732f, -0.415802277f, 0.840000362f), new Vector3(-0.2471778936f, -0.7304819962f, -0.6366310879f), new Vector3(-0.3700154943f, 0.8577948156f, 0.3567584454f),
            new Vector3(0.5913394901f, -0.548311967f, -0.5913303597f), new Vector3(0.1204873514f, -0.7626472379f, -0.6354935001f), new Vector3(0.616959265f, 0.03079647928f, 0.7863922953f), new Vector3(0.1258156836f, -0.6640829889f, -0.7369967419f), new Vector3(-0.6477565124f, -0.1740147258f, -0.7417077429f), new Vector3(0.6217889313f, -0.7804430448f, -0.06547655076f), new Vector3(0.6589943422f, -0.6096987708f, 0.4404473475f), new Vector3(-0.2689837504f, -0.6732403169f, -0.6887635427f),
            new Vector3(-0.3849775103f, 0.5676542638f, 0.7277093879f), new Vector3(0.5754444408f, 0.8110471154f, -0.1051963504f), new Vector3(0.9141593684f, 0.3832947817f, 0.131900567f), new Vector3(-0.107925319f, 0.9245493968f, 0.3654593525f), new Vector3(0.377977089f, 0.3043148782f, 0.8743716458f), new Vector3(-0.2142885215f, -0.8259286236f, 0.5214617324f), new Vector3(0.5802544474f, 0.4148098596f, -0.7008834116f), new Vector3(-0.1982660881f, 0.8567161266f, -0.4761596756f),
            new Vector3(-0.03381553704f, 0.3773180787f, -0.9254661404f), new Vector3(-0.6867922841f, -0.6656597827f, 0.2919133642f), new Vector3(0.7731742607f, -0.2875793547f, -0.5652430251f), new Vector3(-0.09655941928f, 0.9193708367f, -0.3813575004f), new Vector3(0.2715702457f, -0.9577909544f, -0.09426605581f), new Vector3(0.2451015704f, -0.6917998565f, -0.6792188003f), new Vector3(0.977700782f, -0.1753855374f, 0.1155036542f), new Vector3(-0.5224739938f, 0.8521606816f, 0.02903615945f),
            new Vector3(-0.7734880599f, -0.5261292347f, 0.3534179531f), new Vector3(-0.7134492443f, -0.269547243f, 0.6467878011f), new Vector3(0.1644037271f, 0.5105846203f, -0.8439637196f), new Vector3(0.6494635788f, 0.05585611296f, 0.7583384168f), new Vector3(-0.4711970882f, 0.5017280509f, -0.7254255765f), new Vector3(-0.6335764307f, -0.2381686273f, -0.7361091029f), new Vector3(-0.9021533097f, -0.270947803f, -0.3357181763f), new Vector3(-0.3793711033f, 0.872258117f, 0.3086152025f),
            new Vector3(-0.6855598966f, -0.3250143309f, 0.6514394162f), new Vector3(0.2900942212f, -0.7799057743f, -0.5546100667f), new Vector3(-0.2098319339f, 0.85037073f, 0.4825351604f), new Vector3(-0.4592603758f, 0.6598504336f, -0.5947077538f), new Vector3(0.8715945488f, 0.09616365406f, -0.4807031248f), new Vector3(-0.6776666319f, 0.7118504878f, -0.1844907016f), new Vector3(0.7044377633f, 0.312427597f, 0.637304036f), new Vector3(-0.7052318886f, -0.2401093292f, -0.6670798253f),
            new Vector3(0.081921007f, -0.7207336136f, -0.6883545647f), new Vector3(-0.6993680906f, -0.5875763221f, -0.4069869034f), new Vector3(-0.1281454481f, 0.6419895885f, 0.7559286424f), new Vector3(-0.6337388239f, -0.6785471501f, -0.3714146849f), new Vector3(0.5565051903f, -0.2168887573f, -0.8020356851f), new Vector3(-0.5791554484f, 0.7244372011f, -0.3738578718f), new Vector3(0.1175779076f, -0.7096451073f, 0.6946792478f), new Vector3(-0.6134619607f, 0.1323631078f, 0.7785527795f),
            new Vector3(0.6984635305f, -0.02980516237f, -0.715024719f), new Vector3(0.8318082963f, -0.3930171956f, 0.3919597455f), new Vector3(0.1469576422f, 0.05541651717f, -0.9875892167f), new Vector3(0.708868575f, -0.2690503865f, 0.6520101478f), new Vector3(0.2726053183f, 0.67369766f, -0.68688995f), new Vector3(-0.6591295371f, 0.3035458599f, -0.6880466294f), new Vector3(0.4815131379f, -0.7528270071f, 0.4487723203f), new Vector3(0.9430009463f, 0.1675647412f, -0.2875261255f),
            new Vector3(0.434802957f, 0.7695304522f, -0.4677277752f), new Vector3(0.3931996188f, 0.594473625f, 0.7014236729f), new Vector3(0.7254336655f, -0.603925654f, 0.3301814672f), new Vector3(0.7590235227f, -0.6506083235f, 0.02433313207f), new Vector3(-0.8552768592f, -0.3430042733f, 0.3883935666f), new Vector3(-0.6139746835f, 0.6981725247f, 0.3682257648f), new Vector3(-0.7465905486f, -0.5752009504f, 0.3342849376f), new Vector3(0.5730065677f, 0.810555537f, -0.1210916791f),
            new Vector3(-0.9225877367f, -0.3475211012f, -0.167514036f), new Vector3(-0.7105816789f, -0.4719692027f, -0.5218416899f), new Vector3(-0.08564609717f, 0.3583001386f, 0.929669703f), new Vector3(-0.8279697606f, -0.2043157126f, 0.5222271202f), new Vector3(0.427944023f, 0.278165994f, 0.8599346446f), new Vector3(0.5399079671f, -0.7857120652f, -0.3019204161f), new Vector3(0.5678404253f, -0.5495413974f, -0.6128307303f), new Vector3(-0.9896071041f, 0.1365639107f, -0.04503418428f),
            new Vector3(-0.6154342638f, -0.6440875597f, 0.4543037336f), new Vector3(0.1074204368f, -0.7946340692f, 0.5975094525f), new Vector3(-0.3595449969f, -0.8885529948f, 0.28495784f), new Vector3(-0.2180405296f, 0.1529888965f, 0.9638738118f), new Vector3(-0.7277432317f, -0.6164050508f, -0.3007234646f), new Vector3(0.7249729114f, -0.00669719484f, 0.6887448187f), new Vector3(-0.5553659455f, -0.5336586252f, 0.6377908264f), new Vector3(0.5137558015f, 0.7976208196f, -0.3160000073f),
            new Vector3(-0.3794024848f, 0.9245608561f, -0.03522751494f), new Vector3(0.8229248658f, 0.2745365933f, -0.4974176556f), new Vector3(-0.5404114394f, 0.6091141441f, 0.5804613989f), new Vector3(0.8036581901f, -0.2703029469f, 0.5301601931f), new Vector3(0.6044318879f, 0.6832968393f, 0.4095943388f), new Vector3(0.06389988817f, 0.9658208605f, -0.2512108074f), new Vector3(0.1087113286f, 0.7402471173f, -0.6634877936f), new Vector3(-0.713427712f, -0.6926784018f, 0.1059128479f),
            new Vector3(0.6458897819f, -0.5724548511f, -0.5050958653f), new Vector3(-0.6553931414f, 0.7381471625f, 0.159995615f), new Vector3(0.3910961323f, 0.9188871375f, -0.05186755998f), new Vector3(-0.4879022471f, -0.5904376907f, 0.6429111375f), new Vector3(0.6014790094f, 0.7707441366f, -0.2101820095f), new Vector3(-0.5677173047f, 0.7511360995f, 0.3368851762f), new Vector3(0.7858573506f, 0.226674665f, 0.5753666838f), new Vector3(-0.4520345543f, -0.604222686f, -0.6561857263f),
            new Vector3(0.002272116345f, 0.4132844051f, -0.9105991643f), new Vector3(-0.5815751419f, -0.5162925989f, 0.6286591339f), new Vector3(-0.03703704785f, 0.8273785755f, 0.5604221175f), new Vector3(-0.5119692504f, 0.7953543429f, -0.3244980058f), new Vector3(-0.2682417366f, -0.9572290247f, -0.1084387619f), new Vector3(-0.2322482736f, -0.9679131102f, -0.09594243324f), new Vector3(0.3554328906f, -0.8881505545f, 0.2913006227f), new Vector3(0.7346520519f, -0.4371373164f, 0.5188422971f),
            new Vector3(0.9985120116f, 0.04659011161f, -0.02833944577f), new Vector3(-0.3727687496f, -0.9082481361f, 0.1900757285f), new Vector3(0.91737377f, -0.3483642108f, 0.1925298489f), new Vector3(0.2714911074f, 0.4147529736f, -0.8684886582f), new Vector3(0.5131763485f, -0.7116334161f, 0.4798207128f), new Vector3(-0.8737353606f, 0.18886992f, -0.4482350644f), new Vector3(0.8460043821f, -0.3725217914f, 0.3814499973f), new Vector3(0.8978727456f, -0.1780209141f, -0.4026575304f),
            new Vector3(0.2178065647f, -0.9698322841f, -0.1094789531f), new Vector3(-0.1518031304f, -0.7788918132f, -0.6085091231f), new Vector3(-0.2600384876f, -0.4755398075f, -0.8403819825f), new Vector3(0.572313509f, -0.7474340931f, -0.3373418503f), new Vector3(-0.7174141009f, 0.1699017182f, -0.6756111411f), new Vector3(-0.684180784f, 0.02145707593f, -0.7289967412f), new Vector3(-0.2007447902f, 0.06555605789f, -0.9774476623f), new Vector3(-0.1148803697f, -0.8044887315f, 0.5827524187f),
            new Vector3(-0.7870349638f, 0.03447489231f, 0.6159443543f), new Vector3(-0.2015596421f, 0.6859872284f, 0.6991389226f), new Vector3(-0.08581082512f, -0.10920836f, -0.9903080513f), new Vector3(0.5532693395f, 0.7325250401f, -0.396610771f), new Vector3(-0.1842489331f, -0.9777375055f, -0.1004076743f), new Vector3(0.0775473789f, -0.9111505856f, 0.4047110257f), new Vector3(0.1399838409f, 0.7601631212f, -0.6344734459f), new Vector3(0.4484419361f, -0.845289248f, 0.2904925424f),
        };

        private static ReadOnlySpan<byte> Simplex4D => new byte[]
        {
            0,1,2,3,0,1,3,2,0,0,0,0,0,2,3,1,0,0,0,0,0,0,0,0,0,0,0,0,1,2,3,0,
            0,2,1,3,0,0,0,0,0,3,1,2,0,3,2,1,0,0,0,0,0,0,0,0,0,0,0,0,1,3,2,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            1,2,0,3,0,0,0,0,1,3,0,2,0,0,0,0,0,0,0,0,0,0,0,0,2,3,0,1,2,3,1,0,
            1,0,2,3,1,0,3,2,0,0,0,0,0,0,0,0,0,0,0,0,2,0,3,1,0,0,0,0,2,1,3,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            2,0,1,3,0,0,0,0,0,0,0,0,0,0,0,0,3,0,1,2,3,0,2,1,0,0,0,0,3,1,2,0,
            2,1,0,3,0,0,0,0,0,0,0,0,0,0,0,0,3,1,0,2,0,0,0,0,3,2,0,1,3,2,1,0
        };

        private int _octaves = 3;
        private float _gain = 0.5f;
        private float _fractalBounding;
        private int _cellularDistanceIndex0 = 0;
        private int _cellularDistanceIndex1 = 1;

        public int Seed { get; set; }

        public float Frequency { get; set; }

        public Interp Interp { get; set; }

        public NoiseType NoiseType { get; set; }

        public FractalType FractalType { get; set; }

        public CellularDistanceFunction CellularDistanceFunction { get; set; }

        public CellularReturnType CellularReturnType { get; set; }

        public FastNoise CellularNoiseLookup { get; set; }

        public float FractalLacunarity { get; set; }

        public float CellularJitter { get; set; }

        public float GradientPerturbAmp { get; set; }

        public int FractalOctaves
        {
            get => _octaves;
            set
            {
                _octaves = value;

                CalculateFractalBounding();
            }
        }

        public float FractalGain
        {
            get => _gain;
            set
            {
                _gain = value;

                CalculateFractalBounding();
            }
        }

        public FastNoise(int seed = 1337)
        {
            Seed = seed;
            Frequency = 0.01f;
            Interp = Interp.Quintic;
            NoiseType = NoiseType.Simplex;
            FractalType = FractalType.FBM;
            CellularDistanceFunction = CellularDistanceFunction.Euclidean;
            CellularReturnType = CellularReturnType.CellValue;
            CellularNoiseLookup = null;
            FractalLacunarity = 2;
            CellularJitter = 0.45f;
            GradientPerturbAmp = 1;

            CalculateFractalBounding();
        }

        // Sets the 2 distance indicies used for distance2 return types
        // Default: 0, 1
        // Note: index0 should be lower than index1
        // Both indicies must be >= 0, index1 must be < 4
        public void SetCellularDistance2Indicies(int cellularDistanceIndex0, int cellularDistanceIndex1)
        {
            _cellularDistanceIndex0 = Math.Min(cellularDistanceIndex0, cellularDistanceIndex1);
            _cellularDistanceIndex1 = Math.Max(cellularDistanceIndex0, cellularDistanceIndex1);

            _cellularDistanceIndex0 = Math.Min(Math.Max(_cellularDistanceIndex0, 0), CellularIndexMax);
            _cellularDistanceIndex1 = Math.Min(Math.Max(_cellularDistanceIndex1, 0), CellularIndexMax);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FastFloor(float f) => f >= 0 ? (int)f : (int)f - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FastRound(float f) => (f >= 0) ? (int)(f + 0.5f) : (int)(f - 0.5f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Lerp(float a, float b, float t) => a + t * (b - a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InterpHermiteFunc(float t) => t * t * (3 - 2 * t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InterpQuinticFunc(float t) => t * t * t * (t * (t * 6 - 15) + 10);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CubicLerp(float a, float b, float c, float d, float t)
        {
            float p = (d - c) - (a - b);

            return t * t * t * p + t * t * ((a - b) - p) + t * (c - a) + b;
        }

        private void CalculateFractalBounding()
        {
            float amp = _gain;
            float ampFractal = 1;
            for (int i = 1; i < _octaves; i++)
            {
                ampFractal += amp;
                amp *= _gain;
            }
            _fractalBounding = 1 / ampFractal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Hash2D(int seed, int x, int y)
        {
            int hash = seed;
            hash ^= PrimeX * x;
            hash ^= PrimeY * y;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Hash3D(int seed, int x, int y, int z)
        {
            int hash = seed;
            hash ^= PrimeX * x;
            hash ^= PrimeY * y;
            hash ^= PrimeZ * z;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Hash4D(int seed, int x, int y, int z, int w)
        {
            int hash = seed;
            hash ^= PrimeX * x;
            hash ^= PrimeY * y;
            hash ^= PrimeZ * z;
            hash ^= PrimeW * w;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ValCoord2D(int seed, int x, int y)
        {
            int n = seed;
            n ^= PrimeX * x;
            n ^= PrimeY * y;

            return (n * n * n * 60493) / 2147483648f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ValCoord3D(int seed, int x, int y, int z)
        {
            int n = seed;
            n ^= PrimeX * x;
            n ^= PrimeY * y;
            n ^= PrimeZ * z;

            return (n * n * n * 60493) / 2147483648f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ValCoord4D(int seed, int x, int y, int z, int w)
        {
            int n = seed;
            n ^= PrimeX * x;
            n ^= PrimeY * y;
            n ^= PrimeZ * z;
            n ^= PrimeW * w;

            return (n * n * n * 60493) / 2147483648f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GradCoord2D(int seed, int x, int y, float xd, float yd)
        {
            int hash = seed;
            hash ^= PrimeX * x;
            hash ^= PrimeY * y;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            Vector2 g = Grad2D[hash & 7];

            return xd * g.X + yd * g.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GradCoord3D(int seed, int x, int y, int z, float xd, float yd, float zd)
        {
            int hash = seed;
            hash ^= PrimeX * x;
            hash ^= PrimeY * y;
            hash ^= PrimeZ * z;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            Vector3 g = Grad3D[hash & 15];

            return xd * g.X + yd * g.Y + zd * g.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GradCoord4D(int seed, int x, int y, int z, int w, float xd, float yd, float zd, float wd)
        {
            int hash = seed;
            hash ^= PrimeX * x;
            hash ^= PrimeY * y;
            hash ^= PrimeZ * z;
            hash ^= PrimeW * w;

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            hash &= 31;
            float a = yd, b = zd, c = wd;            // X,Y,Z
            switch (hash >> 3)
            {          // OR, DEPENDING ON HIGH ORDER 2 BITS:
                case 1: a = wd; b = xd; c = yd; break;     // W,X,Y
                case 2: a = zd; b = wd; c = xd; break;     // Z,W,X
                case 3: a = yd; b = zd; c = wd; break;     // Y,Z,W
            }
            return ((hash & 4) == 0 ? -a : a) + ((hash & 2) == 0 ? -b : b) + ((hash & 1) == 0 ? -c : c);
        }

        public float GetNoise(float x, float y, float z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            switch (NoiseType)
            {
                case NoiseType.Value:
                    return SingleValue(Seed, x, y, z);
                case NoiseType.ValueFractal:
                    return FractalType switch
                    {
                        FractalType.FBM => SingleValueFractalFBM(x, y, z),
                        FractalType.Billow => SingleValueFractalBillow(x, y, z),
                        FractalType.RigidMulti => SingleValueFractalRigidMulti(x, y, z),
                        _ => 0,
                    };
                case NoiseType.Perlin:
                    return SinglePerlin(Seed, x, y, z);
                case NoiseType.PerlinFractal:
                    return FractalType switch
                    {
                        FractalType.FBM => SinglePerlinFractalFBM(x, y, z),
                        FractalType.Billow => SinglePerlinFractalBillow(x, y, z),
                        FractalType.RigidMulti => SinglePerlinFractalRigidMulti(x, y, z),
                        _ => 0,
                    };
                case NoiseType.Simplex:
                    return SingleSimplex(Seed, x, y, z);
                case NoiseType.SimplexFractal:
                    return FractalType switch
                    {
                        FractalType.FBM => SingleSimplexFractalFBM(x, y, z),
                        FractalType.Billow => SingleSimplexFractalBillow(x, y, z),
                        FractalType.RigidMulti => SingleSimplexFractalRigidMulti(x, y, z),
                        _ => 0,
                    };
                case NoiseType.Cellular:
                    switch (CellularReturnType)
                    {
                        case CellularReturnType.CellValue:
                        case CellularReturnType.NoiseLookup:
                        case CellularReturnType.Distance:
                            return SingleCellular(x, y, z);
                        default:
                            return SingleCellular2Edge(x, y, z);
                    }
                case NoiseType.WhiteNoise:
                    return GetWhiteNoise(x, y, z);
                case NoiseType.Cubic:
                    return SingleCubic(Seed, x, y, z);
                case NoiseType.CubicFractal:
                    return FractalType switch
                    {
                        FractalType.FBM => SingleCubicFractalFBM(x, y, z),
                        FractalType.Billow => SingleCubicFractalBillow(x, y, z),
                        FractalType.RigidMulti => SingleCubicFractalRigidMulti(x, y, z),
                        _ => 0,
                    };
                default:
                    return 0;
            }
        }

        public float GetNoise(float x, float y)
        {
            x *= Frequency;
            y *= Frequency;

            switch (NoiseType)
            {
                case NoiseType.Value:
                    return SingleValue(Seed, x, y);
                case NoiseType.ValueFractal:
                    return FractalType switch
                    {
                        FractalType.FBM => SingleValueFractalFBM(x, y),
                        FractalType.Billow => SingleValueFractalBillow(x, y),
                        FractalType.RigidMulti => SingleValueFractalRigidMulti(x, y),
                        _ => 0,
                    };
                case NoiseType.Perlin:
                    return SinglePerlin(Seed, x, y);
                case NoiseType.PerlinFractal:
                    return FractalType switch
                    {
                        FractalType.FBM => SinglePerlinFractalFBM(x, y),
                        FractalType.Billow => SinglePerlinFractalBillow(x, y),
                        FractalType.RigidMulti => SinglePerlinFractalRigidMulti(x, y),
                        _ => 0,
                    };
                case NoiseType.Simplex:
                    return SingleSimplex(Seed, x, y);
                case NoiseType.SimplexFractal:
                    return FractalType switch
                    {
                        FractalType.FBM => SingleSimplexFractalFBM(x, y),
                        FractalType.Billow => SingleSimplexFractalBillow(x, y),
                        FractalType.RigidMulti => SingleSimplexFractalRigidMulti(x, y),
                        _ => 0,
                    };
                case NoiseType.Cellular:
                    switch (CellularReturnType)
                    {
                        case CellularReturnType.CellValue:
                        case CellularReturnType.NoiseLookup:
                        case CellularReturnType.Distance:
                            return SingleCellular(x, y);
                        default:
                            return SingleCellular2Edge(x, y);
                    }
                case NoiseType.WhiteNoise:
                    return GetWhiteNoise(x, y);
                case NoiseType.Cubic:
                    return SingleCubic(Seed, x, y);
                case NoiseType.CubicFractal:
                    return FractalType switch
                    {
                        FractalType.FBM => SingleCubicFractalFBM(x, y),
                        FractalType.Billow => SingleCubicFractalBillow(x, y),
                        FractalType.RigidMulti => SingleCubicFractalRigidMulti(x, y),
                        _ => 0,
                    };
                default:
                    return 0;
            }
        }

        // White Noise
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FloatCast2Int(float f)
        {
            var i = BitConverter.DoubleToInt64Bits(f);

            return (int)(i ^ (i >> 32));
        }

        public float GetWhiteNoise(float x, float y, float z, float w)
        {
            int xi = FloatCast2Int(x);
            int yi = FloatCast2Int(y);
            int zi = FloatCast2Int(z);
            int wi = FloatCast2Int(w);

            return ValCoord4D(Seed, xi, yi, zi, wi);
        }

        public float GetWhiteNoise(float x, float y, float z)
        {
            int xi = FloatCast2Int(x);
            int yi = FloatCast2Int(y);
            int zi = FloatCast2Int(z);

            return ValCoord3D(Seed, xi, yi, zi);
        }

        public float GetWhiteNoise(float x, float y)
        {
            int xi = FloatCast2Int(x);
            int yi = FloatCast2Int(y);

            return ValCoord2D(Seed, xi, yi);
        }

        public float GetWhiteNoiseInt(int x, int y, int z, int w)
        {
            return ValCoord4D(Seed, x, y, z, w);
        }

        public float GetWhiteNoiseInt(int x, int y, int z)
        {
            return ValCoord3D(Seed, x, y, z);
        }

        public float GetWhiteNoiseInt(int x, int y)
        {
            return ValCoord2D(Seed, x, y);
        }

        // Value Noise
        public float GetValueFractal(float x, float y, float z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            return FractalType switch
            {
                FractalType.FBM => SingleValueFractalFBM(x, y, z),
                FractalType.Billow => SingleValueFractalBillow(x, y, z),
                FractalType.RigidMulti => SingleValueFractalRigidMulti(x, y, z),
                _ => 0,
            };
        }

        private float SingleValueFractalFBM(float x, float y, float z)
        {
            int seed = Seed;
            float sum = SingleValue(seed, x, y, z);
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum += SingleValue(++seed, x, y, z) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleValueFractalBillow(float x, float y, float z)
        {
            int seed = Seed;
            float sum = Math.Abs(SingleValue(seed, x, y, z)) * 2 - 1;
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum += (Math.Abs(SingleValue(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleValueFractalRigidMulti(float x, float y, float z)
        {
            int seed = Seed;
            float sum = 1 - Math.Abs(SingleValue(seed, x, y, z));
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum -= (1 - Math.Abs(SingleValue(++seed, x, y, z))) * amp;
            }

            return sum;
        }

        public float GetValue(float x, float y, float z)
        {
            return SingleValue(Seed, x * Frequency, y * Frequency, z * Frequency);
        }

        private float SingleValue(int seed, float x, float y, float z)
        {
            int x0 = FastFloor(x);
            int y0 = FastFloor(y);
            int z0 = FastFloor(z);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            int z1 = z0 + 1;

            float xs, ys, zs;
            switch (Interp)
            {
                default:
                case Interp.Linear:
                    xs = x - x0;
                    ys = y - y0;
                    zs = z - z0;
                    break;
                case Interp.Hermite:
                    xs = InterpHermiteFunc(x - x0);
                    ys = InterpHermiteFunc(y - y0);
                    zs = InterpHermiteFunc(z - z0);
                    break;
                case Interp.Quintic:
                    xs = InterpQuinticFunc(x - x0);
                    ys = InterpQuinticFunc(y - y0);
                    zs = InterpQuinticFunc(z - z0);
                    break;
            }

            float xf00 = Lerp(ValCoord3D(seed, x0, y0, z0), ValCoord3D(seed, x1, y0, z0), xs);
            float xf10 = Lerp(ValCoord3D(seed, x0, y1, z0), ValCoord3D(seed, x1, y1, z0), xs);
            float xf01 = Lerp(ValCoord3D(seed, x0, y0, z1), ValCoord3D(seed, x1, y0, z1), xs);
            float xf11 = Lerp(ValCoord3D(seed, x0, y1, z1), ValCoord3D(seed, x1, y1, z1), xs);

            float yf0 = Lerp(xf00, xf10, ys);
            float yf1 = Lerp(xf01, xf11, ys);

            return Lerp(yf0, yf1, zs);
        }

        public float GetValueFractal(float x, float y)
        {
            x *= Frequency;
            y *= Frequency;

            return FractalType switch
            {
                FractalType.FBM => SingleValueFractalFBM(x, y),
                FractalType.Billow => SingleValueFractalBillow(x, y),
                FractalType.RigidMulti => SingleValueFractalRigidMulti(x, y),
                _ => 0,
            };
        }

        private float SingleValueFractalFBM(float x, float y)
        {
            int seed = Seed;
            float sum = SingleValue(seed, x, y);
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum += SingleValue(++seed, x, y) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleValueFractalBillow(float x, float y)
        {
            int seed = Seed;
            float sum = Math.Abs(SingleValue(seed, x, y)) * 2 - 1;
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                amp *= _gain;
                sum += (Math.Abs(SingleValue(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleValueFractalRigidMulti(float x, float y)
        {
            int seed = Seed;
            float sum = 1 - Math.Abs(SingleValue(seed, x, y));
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum -= (1 - Math.Abs(SingleValue(++seed, x, y))) * amp;
            }

            return sum;
        }

        public float GetValue(float x, float y)
        {
            return SingleValue(Seed, x * Frequency, y * Frequency);
        }

        private float SingleValue(int seed, float x, float y)
        {
            int x0 = FastFloor(x);
            int y0 = FastFloor(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float xs, ys;
            switch (Interp)
            {
                default:
                case Interp.Linear:
                    xs = x - x0;
                    ys = y - y0;
                    break;
                case Interp.Hermite:
                    xs = InterpHermiteFunc(x - x0);
                    ys = InterpHermiteFunc(y - y0);
                    break;
                case Interp.Quintic:
                    xs = InterpQuinticFunc(x - x0);
                    ys = InterpQuinticFunc(y - y0);
                    break;
            }

            float xf0 = Lerp(ValCoord2D(seed, x0, y0), ValCoord2D(seed, x1, y0), xs);
            float xf1 = Lerp(ValCoord2D(seed, x0, y1), ValCoord2D(seed, x1, y1), xs);

            return Lerp(xf0, xf1, ys);
        }

        // Gradient Noise
        public float GetPerlinFractal(float x, float y, float z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            return FractalType switch
            {
                FractalType.FBM => SinglePerlinFractalFBM(x, y, z),
                FractalType.Billow => SinglePerlinFractalBillow(x, y, z),
                FractalType.RigidMulti => SinglePerlinFractalRigidMulti(x, y, z),
                _ => 0,
            };
        }

        private float SinglePerlinFractalFBM(float x, float y, float z)
        {
            int seed = Seed;
            float sum = SinglePerlin(seed, x, y, z);
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum += SinglePerlin(++seed, x, y, z) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SinglePerlinFractalBillow(float x, float y, float z)
        {
            int seed = Seed;
            float sum = Math.Abs(SinglePerlin(seed, x, y, z)) * 2 - 1;
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum += (Math.Abs(SinglePerlin(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SinglePerlinFractalRigidMulti(float x, float y, float z)
        {
            int seed = Seed;
            float sum = 1 - Math.Abs(SinglePerlin(seed, x, y, z));
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum -= (1 - Math.Abs(SinglePerlin(++seed, x, y, z))) * amp;
            }

            return sum;
        }

        public float GetPerlin(float x, float y, float z)
        {
            return SinglePerlin(Seed, x * Frequency, y * Frequency, z * Frequency);
        }

        private float SinglePerlin(int seed, float x, float y, float z)
        {
            int x0 = FastFloor(x);
            int y0 = FastFloor(y);
            int z0 = FastFloor(z);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            int z1 = z0 + 1;

            float xs, ys, zs;
            switch (Interp)
            {
                default:
                case Interp.Linear:
                    xs = x - x0;
                    ys = y - y0;
                    zs = z - z0;
                    break;
                case Interp.Hermite:
                    xs = InterpHermiteFunc(x - x0);
                    ys = InterpHermiteFunc(y - y0);
                    zs = InterpHermiteFunc(z - z0);
                    break;
                case Interp.Quintic:
                    xs = InterpQuinticFunc(x - x0);
                    ys = InterpQuinticFunc(y - y0);
                    zs = InterpQuinticFunc(z - z0);
                    break;
            }

            float xd0 = x - x0;
            float yd0 = y - y0;
            float zd0 = z - z0;
            float xd1 = xd0 - 1;
            float yd1 = yd0 - 1;
            float zd1 = zd0 - 1;

            float xf00 = Lerp(GradCoord3D(seed, x0, y0, z0, xd0, yd0, zd0), GradCoord3D(seed, x1, y0, z0, xd1, yd0, zd0), xs);
            float xf10 = Lerp(GradCoord3D(seed, x0, y1, z0, xd0, yd1, zd0), GradCoord3D(seed, x1, y1, z0, xd1, yd1, zd0), xs);
            float xf01 = Lerp(GradCoord3D(seed, x0, y0, z1, xd0, yd0, zd1), GradCoord3D(seed, x1, y0, z1, xd1, yd0, zd1), xs);
            float xf11 = Lerp(GradCoord3D(seed, x0, y1, z1, xd0, yd1, zd1), GradCoord3D(seed, x1, y1, z1, xd1, yd1, zd1), xs);

            float yf0 = Lerp(xf00, xf10, ys);
            float yf1 = Lerp(xf01, xf11, ys);

            return Lerp(yf0, yf1, zs);
        }

        public float GetPerlinFractal(float x, float y)
        {
            x *= Frequency;
            y *= Frequency;

            return FractalType switch
            {
                FractalType.FBM => SinglePerlinFractalFBM(x, y),
                FractalType.Billow => SinglePerlinFractalBillow(x, y),
                FractalType.RigidMulti => SinglePerlinFractalRigidMulti(x, y),
                _ => 0,
            };
        }

        private float SinglePerlinFractalFBM(float x, float y)
        {
            int seed = Seed;
            float sum = SinglePerlin(seed, x, y);
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum += SinglePerlin(++seed, x, y) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SinglePerlinFractalBillow(float x, float y)
        {
            int seed = Seed;
            float sum = Math.Abs(SinglePerlin(seed, x, y)) * 2 - 1;
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum += (Math.Abs(SinglePerlin(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SinglePerlinFractalRigidMulti(float x, float y)
        {
            int seed = Seed;
            float sum = 1 - Math.Abs(SinglePerlin(seed, x, y));
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum -= (1 - Math.Abs(SinglePerlin(++seed, x, y))) * amp;
            }

            return sum;
        }

        public float GetPerlin(float x, float y)
        {
            return SinglePerlin(Seed, x * Frequency, y * Frequency);
        }

        private float SinglePerlin(int seed, float x, float y)
        {
            int x0 = FastFloor(x);
            int y0 = FastFloor(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float xs, ys;
            switch (Interp)
            {
                default:
                case Interp.Linear:
                    xs = x - x0;
                    ys = y - y0;
                    break;
                case Interp.Hermite:
                    xs = InterpHermiteFunc(x - x0);
                    ys = InterpHermiteFunc(y - y0);
                    break;
                case Interp.Quintic:
                    xs = InterpQuinticFunc(x - x0);
                    ys = InterpQuinticFunc(y - y0);
                    break;
            }

            float xd0 = x - x0;
            float yd0 = y - y0;
            float xd1 = xd0 - 1;
            float yd1 = yd0 - 1;

            float xf0 = Lerp(GradCoord2D(seed, x0, y0, xd0, yd0), GradCoord2D(seed, x1, y0, xd1, yd0), xs);
            float xf1 = Lerp(GradCoord2D(seed, x0, y1, xd0, yd1), GradCoord2D(seed, x1, y1, xd1, yd1), xs);

            return Lerp(xf0, xf1, ys);
        }

        // Simplex Noise
        public float GetSimplexFractal(float x, float y, float z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            return FractalType switch
            {
                FractalType.FBM => SingleSimplexFractalFBM(x, y, z),
                FractalType.Billow => SingleSimplexFractalBillow(x, y, z),
                FractalType.RigidMulti => SingleSimplexFractalRigidMulti(x, y, z),
                _ => 0,
            };
        }

        private float SingleSimplexFractalFBM(float x, float y, float z)
        {
            int seed = Seed;
            float sum = SingleSimplex(seed, x, y, z);
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum += SingleSimplex(++seed, x, y, z) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleSimplexFractalBillow(float x, float y, float z)
        {
            int seed = Seed;
            float sum = Math.Abs(SingleSimplex(seed, x, y, z)) * 2 - 1;
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum += (Math.Abs(SingleSimplex(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleSimplexFractalRigidMulti(float x, float y, float z)
        {
            int seed = Seed;
            float sum = 1 - Math.Abs(SingleSimplex(seed, x, y, z));
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum -= (1 - Math.Abs(SingleSimplex(++seed, x, y, z))) * amp;
            }

            return sum;
        }

        public float GetSimplex(float x, float y, float z)
        {
            return SingleSimplex(Seed, x * Frequency, y * Frequency, z * Frequency);
        }

        private float SingleSimplex(int seed, float x, float y, float z)
        {
            float t = (x + y + z) * F3;
            int i = FastFloor(x + t);
            int j = FastFloor(y + t);
            int k = FastFloor(z + t);

            t = (i + j + k) * G3;
            float x0 = x - (i - t);
            float y0 = y - (j - t);
            float z0 = z - (k - t);

            int i1, j1, k1;
            int i2, j2, k2;

            if (x0 >= y0)
            {
                if (y0 >= z0)
                {
                    i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
                }
                else if (x0 >= z0)
                {
                    i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1;
                }
                else // x0 < z0
                {
                    i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1;
                }
            }
            else // x0 < y0
            {
                if (y0 < z0)
                {
                    i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1;
                }
                else if (x0 < z0)
                {
                    i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1;
                }
                else // x0 >= z0
                {
                    i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
                }
            }

            float x1 = x0 - i1 + G3;
            float y1 = y0 - j1 + G3;
            float z1 = z0 - k1 + G3;
            float x2 = x0 - i2 + F3;
            float y2 = y0 - j2 + F3;
            float z2 = z0 - k2 + F3;
            float x3 = x0 + G33;
            float y3 = y0 + G33;
            float z3 = z0 + G33;

            float n0, n1, n2, n3;

            t = (float)0.6 - x0 * x0 - y0 * y0 - z0 * z0;
            if (t < 0) n0 = 0;
            else
            {
                t *= t;
                n0 = t * t * GradCoord3D(seed, i, j, k, x0, y0, z0);
            }

            t = (float)0.6 - x1 * x1 - y1 * y1 - z1 * z1;
            if (t < 0) n1 = 0;
            else
            {
                t *= t;
                n1 = t * t * GradCoord3D(seed, i + i1, j + j1, k + k1, x1, y1, z1);
            }

            t = (float)0.6 - x2 * x2 - y2 * y2 - z2 * z2;
            if (t < 0) n2 = 0;
            else
            {
                t *= t;
                n2 = t * t * GradCoord3D(seed, i + i2, j + j2, k + k2, x2, y2, z2);
            }

            t = (float)0.6 - x3 * x3 - y3 * y3 - z3 * z3;
            if (t < 0) n3 = 0;
            else
            {
                t *= t;
                n3 = t * t * GradCoord3D(seed, i + 1, j + 1, k + 1, x3, y3, z3);
            }

            return 32 * (n0 + n1 + n2 + n3);
        }

        public float GetSimplexFractal(float x, float y)
        {
            x *= Frequency;
            y *= Frequency;

            return FractalType switch
            {
                FractalType.FBM => SingleSimplexFractalFBM(x, y),
                FractalType.Billow => SingleSimplexFractalBillow(x, y),
                FractalType.RigidMulti => SingleSimplexFractalRigidMulti(x, y),
                _ => 0,
            };
        }

        private float SingleSimplexFractalFBM(float x, float y)
        {
            int seed = Seed;
            float sum = SingleSimplex(seed, x, y);
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum += SingleSimplex(++seed, x, y) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleSimplexFractalBillow(float x, float y)
        {
            int seed = Seed;
            float sum = Math.Abs(SingleSimplex(seed, x, y)) * 2 - 1;
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum += (Math.Abs(SingleSimplex(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleSimplexFractalRigidMulti(float x, float y)
        {
            int seed = Seed;
            float sum = 1 - Math.Abs(SingleSimplex(seed, x, y));
            float amp = 1;

            for (int i = 1; i < _octaves; i++)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum -= (1 - Math.Abs(SingleSimplex(++seed, x, y))) * amp;
            }

            return sum;
        }

        public float GetSimplex(float x, float y)
        {
            return SingleSimplex(Seed, x * Frequency, y * Frequency);
        }

        private float SingleSimplex(int seed, float x, float y)
        {
            float t = (x + y) * F2;
            int i = FastFloor(x + t);
            int j = FastFloor(y + t);

            t = (i + j) * G2;
            float X0 = i - t;
            float Y0 = j - t;

            float x0 = x - X0;
            float y0 = y - Y0;

            int i1, j1;
            if (x0 > y0)
            {
                i1 = 1; j1 = 0;
            }
            else
            {
                i1 = 0; j1 = 1;
            }

            float x1 = x0 - i1 + G2;
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1 + 2 * G2;
            float y2 = y0 - 1 + 2 * G2;

            float n0, n1, n2;

            t = (float)0.5 - x0 * x0 - y0 * y0;
            if (t < 0) n0 = 0;
            else
            {
                t *= t;
                n0 = t * t * GradCoord2D(seed, i, j, x0, y0);
            }

            t = (float)0.5 - x1 * x1 - y1 * y1;
            if (t < 0) n1 = 0;
            else
            {
                t *= t;
                n1 = t * t * GradCoord2D(seed, i + i1, j + j1, x1, y1);
            }

            t = (float)0.5 - x2 * x2 - y2 * y2;
            if (t < 0) n2 = 0;
            else
            {
                t *= t;
                n2 = t * t * GradCoord2D(seed, i + 1, j + 1, x2, y2);
            }

            return 50 * (n0 + n1 + n2);
        }

        public float GetSimplex(float x, float y, float z, float w)
        {
            return SingleSimplex(Seed, x * Frequency, y * Frequency, z * Frequency, w * Frequency);
        }

        private float SingleSimplex(int seed, float x, float y, float z, float w)
        {
            float n0, n1, n2, n3, n4;
            float t = (x + y + z + w) * F4;
            int i = FastFloor(x + t);
            int j = FastFloor(y + t);
            int k = FastFloor(z + t);
            int l = FastFloor(w + t);
            t = (i + j + k + l) * G4;
            float X0 = i - t;
            float Y0 = j - t;
            float Z0 = k - t;
            float W0 = l - t;
            float x0 = x - X0;
            float y0 = y - Y0;
            float z0 = z - Z0;
            float w0 = w - W0;

            int c = (x0 > y0) ? 32 : 0;
            c += (x0 > z0) ? 16 : 0;
            c += (y0 > z0) ? 8 : 0;
            c += (x0 > w0) ? 4 : 0;
            c += (y0 > w0) ? 2 : 0;
            c += (z0 > w0) ? 1 : 0;
            c <<= 2;

            int i1 = Simplex4D[c] >= 3 ? 1 : 0;
            int i2 = Simplex4D[c] >= 2 ? 1 : 0;
            int i3 = Simplex4D[c++] >= 1 ? 1 : 0;
            int j1 = Simplex4D[c] >= 3 ? 1 : 0;
            int j2 = Simplex4D[c] >= 2 ? 1 : 0;
            int j3 = Simplex4D[c++] >= 1 ? 1 : 0;
            int k1 = Simplex4D[c] >= 3 ? 1 : 0;
            int k2 = Simplex4D[c] >= 2 ? 1 : 0;
            int k3 = Simplex4D[c++] >= 1 ? 1 : 0;
            int l1 = Simplex4D[c] >= 3 ? 1 : 0;
            int l2 = Simplex4D[c] >= 2 ? 1 : 0;
            int l3 = Simplex4D[c] >= 1 ? 1 : 0;

            float x1 = x0 - i1 + G4;
            float y1 = y0 - j1 + G4;
            float z1 = z0 - k1 + G4;
            float w1 = w0 - l1 + G4;
            float x2 = x0 - i2 + 2 * G4;
            float y2 = y0 - j2 + 2 * G4;
            float z2 = z0 - k2 + 2 * G4;
            float w2 = w0 - l2 + 2 * G4;
            float x3 = x0 - i3 + 3 * G4;
            float y3 = y0 - j3 + 3 * G4;
            float z3 = z0 - k3 + 3 * G4;
            float w3 = w0 - l3 + 3 * G4;
            float x4 = x0 - 1 + 4 * G4;
            float y4 = y0 - 1 + 4 * G4;
            float z4 = z0 - 1 + 4 * G4;
            float w4 = w0 - 1 + 4 * G4;

            t = (float)0.6 - x0 * x0 - y0 * y0 - z0 * z0 - w0 * w0;
            if (t < 0) n0 = 0;
            else
            {
                t *= t;
                n0 = t * t * GradCoord4D(seed, i, j, k, l, x0, y0, z0, w0);
            }
            t = (float)0.6 - x1 * x1 - y1 * y1 - z1 * z1 - w1 * w1;
            if (t < 0) n1 = 0;
            else
            {
                t *= t;
                n1 = t * t * GradCoord4D(seed, i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1);
            }
            t = (float)0.6 - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;
            if (t < 0) n2 = 0;
            else
            {
                t *= t;
                n2 = t * t * GradCoord4D(seed, i + i2, j + j2, k + k2, l + l2, x2, y2, z2, w2);
            }
            t = (float)0.6 - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;
            if (t < 0) n3 = 0;
            else
            {
                t *= t;
                n3 = t * t * GradCoord4D(seed, i + i3, j + j3, k + k3, l + l3, x3, y3, z3, w3);
            }
            t = (float)0.6 - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;
            if (t < 0) n4 = 0;
            else
            {
                t *= t;
                n4 = t * t * GradCoord4D(seed, i + 1, j + 1, k + 1, l + 1, x4, y4, z4, w4);
            }

            return 27 * (n0 + n1 + n2 + n3 + n4);
        }

        // Cubic Noise
        public float GetCubicFractal(float x, float y, float z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            return FractalType switch
            {
                FractalType.FBM => SingleCubicFractalFBM(x, y, z),
                FractalType.Billow => SingleCubicFractalBillow(x, y, z),
                FractalType.RigidMulti => SingleCubicFractalRigidMulti(x, y, z),
                _ => 0,
            };
        }

        private float SingleCubicFractalFBM(float x, float y, float z)
        {
            int seed = Seed;
            float sum = SingleCubic(seed, x, y, z);
            float amp = 1;
            int i = 0;

            while (++i < _octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum += SingleCubic(++seed, x, y, z) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleCubicFractalBillow(float x, float y, float z)
        {
            int seed = Seed;
            float sum = Math.Abs(SingleCubic(seed, x, y, z)) * 2 - 1;
            float amp = 1;
            int i = 0;

            while (++i < _octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum += (Math.Abs(SingleCubic(++seed, x, y, z)) * 2 - 1) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleCubicFractalRigidMulti(float x, float y, float z)
        {
            int seed = Seed;
            float sum = 1 - Math.Abs(SingleCubic(seed, x, y, z));
            float amp = 1;
            int i = 0;

            while (++i < _octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;
                z *= FractalLacunarity;

                amp *= _gain;
                sum -= (1 - Math.Abs(SingleCubic(++seed, x, y, z))) * amp;
            }

            return sum;
        }

        public float GetCubic(float x, float y, float z)
        {
            return SingleCubic(Seed, x * Frequency, y * Frequency, z * Frequency);
        }

        private float SingleCubic(int seed, float x, float y, float z)
        {
            int x1 = FastFloor(x);
            int y1 = FastFloor(y);
            int z1 = FastFloor(z);

            int x0 = x1 - 1;
            int y0 = y1 - 1;
            int z0 = z1 - 1;
            int x2 = x1 + 1;
            int y2 = y1 + 1;
            int z2 = z1 + 1;
            int x3 = x1 + 2;
            int y3 = y1 + 2;
            int z3 = z1 + 2;

            float xs = x - (float)x1;
            float ys = y - (float)y1;
            float zs = z - (float)z1;

            return CubicLerp(
                CubicLerp(
                CubicLerp(ValCoord3D(seed, x0, y0, z0), ValCoord3D(seed, x1, y0, z0), ValCoord3D(seed, x2, y0, z0), ValCoord3D(seed, x3, y0, z0), xs),
                CubicLerp(ValCoord3D(seed, x0, y1, z0), ValCoord3D(seed, x1, y1, z0), ValCoord3D(seed, x2, y1, z0), ValCoord3D(seed, x3, y1, z0), xs),
                CubicLerp(ValCoord3D(seed, x0, y2, z0), ValCoord3D(seed, x1, y2, z0), ValCoord3D(seed, x2, y2, z0), ValCoord3D(seed, x3, y2, z0), xs),
                CubicLerp(ValCoord3D(seed, x0, y3, z0), ValCoord3D(seed, x1, y3, z0), ValCoord3D(seed, x2, y3, z0), ValCoord3D(seed, x3, y3, z0), xs),
                ys),
                CubicLerp(
                CubicLerp(ValCoord3D(seed, x0, y0, z1), ValCoord3D(seed, x1, y0, z1), ValCoord3D(seed, x2, y0, z1), ValCoord3D(seed, x3, y0, z1), xs),
                CubicLerp(ValCoord3D(seed, x0, y1, z1), ValCoord3D(seed, x1, y1, z1), ValCoord3D(seed, x2, y1, z1), ValCoord3D(seed, x3, y1, z1), xs),
                CubicLerp(ValCoord3D(seed, x0, y2, z1), ValCoord3D(seed, x1, y2, z1), ValCoord3D(seed, x2, y2, z1), ValCoord3D(seed, x3, y2, z1), xs),
                CubicLerp(ValCoord3D(seed, x0, y3, z1), ValCoord3D(seed, x1, y3, z1), ValCoord3D(seed, x2, y3, z1), ValCoord3D(seed, x3, y3, z1), xs),
                ys),
                CubicLerp(
                CubicLerp(ValCoord3D(seed, x0, y0, z2), ValCoord3D(seed, x1, y0, z2), ValCoord3D(seed, x2, y0, z2), ValCoord3D(seed, x3, y0, z2), xs),
                CubicLerp(ValCoord3D(seed, x0, y1, z2), ValCoord3D(seed, x1, y1, z2), ValCoord3D(seed, x2, y1, z2), ValCoord3D(seed, x3, y1, z2), xs),
                CubicLerp(ValCoord3D(seed, x0, y2, z2), ValCoord3D(seed, x1, y2, z2), ValCoord3D(seed, x2, y2, z2), ValCoord3D(seed, x3, y2, z2), xs),
                CubicLerp(ValCoord3D(seed, x0, y3, z2), ValCoord3D(seed, x1, y3, z2), ValCoord3D(seed, x2, y3, z2), ValCoord3D(seed, x3, y3, z2), xs),
                ys),
                CubicLerp(
                CubicLerp(ValCoord3D(seed, x0, y0, z3), ValCoord3D(seed, x1, y0, z3), ValCoord3D(seed, x2, y0, z3), ValCoord3D(seed, x3, y0, z3), xs),
                CubicLerp(ValCoord3D(seed, x0, y1, z3), ValCoord3D(seed, x1, y1, z3), ValCoord3D(seed, x2, y1, z3), ValCoord3D(seed, x3, y1, z3), xs),
                CubicLerp(ValCoord3D(seed, x0, y2, z3), ValCoord3D(seed, x1, y2, z3), ValCoord3D(seed, x2, y2, z3), ValCoord3D(seed, x3, y2, z3), xs),
                CubicLerp(ValCoord3D(seed, x0, y3, z3), ValCoord3D(seed, x1, y3, z3), ValCoord3D(seed, x2, y3, z3), ValCoord3D(seed, x3, y3, z3), xs),
                ys),
                zs) * Cubic3DBounding;
        }


        public float GetCubicFractal(float x, float y)
        {
            x *= Frequency;
            y *= Frequency;

            return FractalType switch
            {
                FractalType.FBM => SingleCubicFractalFBM(x, y),
                FractalType.Billow => SingleCubicFractalBillow(x, y),
                FractalType.RigidMulti => SingleCubicFractalRigidMulti(x, y),
                _ => 0,
            };
        }

        private float SingleCubicFractalFBM(float x, float y)
        {
            int seed = Seed;
            float sum = SingleCubic(seed, x, y);
            float amp = 1;
            int i = 0;

            while (++i < _octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum += SingleCubic(++seed, x, y) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleCubicFractalBillow(float x, float y)
        {
            int seed = Seed;
            float sum = Math.Abs(SingleCubic(seed, x, y)) * 2 - 1;
            float amp = 1;
            int i = 0;

            while (++i < _octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum += (Math.Abs(SingleCubic(++seed, x, y)) * 2 - 1) * amp;
            }

            return sum * _fractalBounding;
        }

        private float SingleCubicFractalRigidMulti(float x, float y)
        {
            int seed = Seed;
            float sum = 1 - Math.Abs(SingleCubic(seed, x, y));
            float amp = 1;
            int i = 0;

            while (++i < _octaves)
            {
                x *= FractalLacunarity;
                y *= FractalLacunarity;

                amp *= _gain;
                sum -= (1 - Math.Abs(SingleCubic(++seed, x, y))) * amp;
            }

            return sum;
        }

        public float GetCubic(float x, float y)
        {
            x *= Frequency;
            y *= Frequency;

            return SingleCubic(0, x, y);
        }

        private float SingleCubic(int seed, float x, float y)
        {
            int x1 = FastFloor(x);
            int y1 = FastFloor(y);

            int x0 = x1 - 1;
            int y0 = y1 - 1;
            int x2 = x1 + 1;
            int y2 = y1 + 1;
            int x3 = x1 + 2;
            int y3 = y1 + 2;

            float xs = x - (float)x1;
            float ys = y - (float)y1;

            return CubicLerp(
                       CubicLerp(ValCoord2D(seed, x0, y0), ValCoord2D(seed, x1, y0), ValCoord2D(seed, x2, y0), ValCoord2D(seed, x3, y0),
                           xs),
                       CubicLerp(ValCoord2D(seed, x0, y1), ValCoord2D(seed, x1, y1), ValCoord2D(seed, x2, y1), ValCoord2D(seed, x3, y1),
                           xs),
                       CubicLerp(ValCoord2D(seed, x0, y2), ValCoord2D(seed, x1, y2), ValCoord2D(seed, x2, y2), ValCoord2D(seed, x3, y2),
                           xs),
                       CubicLerp(ValCoord2D(seed, x0, y3), ValCoord2D(seed, x1, y3), ValCoord2D(seed, x2, y3), ValCoord2D(seed, x3, y3),
                           xs),
                       ys) * Cubic2DBounding;
        }

        // Cellular Noise
        public float GetCellular(float x, float y, float z)
        {
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;

            switch (CellularReturnType)
            {
                case CellularReturnType.CellValue:
                case CellularReturnType.NoiseLookup:
                case CellularReturnType.Distance:
                    return SingleCellular(x, y, z);
                default:
                    return SingleCellular2Edge(x, y, z);
            }
        }

        private float SingleCellular(float x, float y, float z)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);
            int zr = FastRound(z);

            float distance = 999999;
            int xc = 0, yc = 0, zc = 0;

            switch (CellularDistanceFunction)
            {
                case CellularDistanceFunction.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Vector3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                float vecX = xi - x + vec.X * CellularJitter;
                                float vecY = yi - y + vec.Y * CellularJitter;
                                float vecZ = zi - z + vec.Z * CellularJitter;

                                float newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

                                if (newDistance < distance)
                                {
                                    distance = newDistance;
                                    xc = xi;
                                    yc = yi;
                                    zc = zi;
                                }
                            }
                        }
                    }
                    break;
                case CellularDistanceFunction.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Vector3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                float vecX = xi - x + vec.X * CellularJitter;
                                float vecY = yi - y + vec.Y * CellularJitter;
                                float vecZ = zi - z + vec.Z * CellularJitter;

                                float newDistance = Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ);

                                if (newDistance < distance)
                                {
                                    distance = newDistance;
                                    xc = xi;
                                    yc = yi;
                                    zc = zi;
                                }
                            }
                        }
                    }
                    break;
                case CellularDistanceFunction.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Vector3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                float vecX = xi - x + vec.X * CellularJitter;
                                float vecY = yi - y + vec.Y * CellularJitter;
                                float vecZ = zi - z + vec.Z * CellularJitter;

                                float newDistance = (Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ)) + (vecX * vecX + vecY * vecY + vecZ * vecZ);

                                if (newDistance < distance)
                                {
                                    distance = newDistance;
                                    xc = xi;
                                    yc = yi;
                                    zc = zi;
                                }
                            }
                        }
                    }
                    break;
            }

            switch (CellularReturnType)
            {
                case CellularReturnType.CellValue:
                    return ValCoord3D(Seed, xc, yc, zc);

                case CellularReturnType.NoiseLookup:
                    Vector3 vec = Cell3D[Hash3D(Seed, xc, yc, zc) & 255];
                    return CellularNoiseLookup.GetNoise(xc + vec.X * CellularJitter, yc + vec.Y * CellularJitter, zc + vec.Z * CellularJitter);

                case CellularReturnType.Distance:
                    return distance;
                default:
                    return 0;
            }
        }

        private float SingleCellular2Edge(float x, float y, float z)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);
            int zr = FastRound(z);

            float[] distance = { 999999, 999999, 999999, 999999 };

            switch (CellularDistanceFunction)
            {
                case CellularDistanceFunction.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Vector3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                float vecX = xi - x + vec.X * CellularJitter;
                                float vecY = yi - y + vec.Y * CellularJitter;
                                float vecZ = zi - z + vec.Z * CellularJitter;

                                float newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

                                for (int i = _cellularDistanceIndex1; i > 0; i--)
                                    distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                                distance[0] = Math.Min(distance[0], newDistance);
                            }
                        }
                    }
                    break;
                case CellularDistanceFunction.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Vector3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                float vecX = xi - x + vec.X * CellularJitter;
                                float vecY = yi - y + vec.Y * CellularJitter;
                                float vecZ = zi - z + vec.Z * CellularJitter;

                                float newDistance = Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ);

                                for (int i = _cellularDistanceIndex1; i > 0; i--)
                                    distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                                distance[0] = Math.Min(distance[0], newDistance);
                            }
                        }
                    }
                    break;
                case CellularDistanceFunction.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            for (int zi = zr - 1; zi <= zr + 1; zi++)
                            {
                                Vector3 vec = Cell3D[Hash3D(Seed, xi, yi, zi) & 255];

                                float vecX = xi - x + vec.X * CellularJitter;
                                float vecY = yi - y + vec.Y * CellularJitter;
                                float vecZ = zi - z + vec.Z * CellularJitter;

                                float newDistance = (Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ)) + (vecX * vecX + vecY * vecY + vecZ * vecZ);

                                for (int i = _cellularDistanceIndex1; i > 0; i--)
                                    distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                                distance[0] = Math.Min(distance[0], newDistance);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return CellularReturnType switch
            {
                CellularReturnType.Distance2 => distance[_cellularDistanceIndex1],
                CellularReturnType.Distance2Add => distance[_cellularDistanceIndex1] + distance[_cellularDistanceIndex0],
                CellularReturnType.Distance2Sub => distance[_cellularDistanceIndex1] - distance[_cellularDistanceIndex0],
                CellularReturnType.Distance2Mul => distance[_cellularDistanceIndex1] * distance[_cellularDistanceIndex0],
                CellularReturnType.Distance2Div => distance[_cellularDistanceIndex0] / distance[_cellularDistanceIndex1],
                _ => 0,
            };
        }

        public float GetCellular(float x, float y)
        {
            x *= Frequency;
            y *= Frequency;

            switch (CellularReturnType)
            {
                case CellularReturnType.CellValue:
                case CellularReturnType.NoiseLookup:
                case CellularReturnType.Distance:
                    return SingleCellular(x, y);
                default:
                    return SingleCellular2Edge(x, y);
            }
        }

        private float SingleCellular(float x, float y)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);

            float distance = 999999;
            int xc = 0, yc = 0;

            switch (CellularDistanceFunction)
            {
                default:
                case CellularDistanceFunction.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Vector2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            float vecX = xi - x + vec.X * CellularJitter;
                            float vecY = yi - y + vec.Y * CellularJitter;

                            float newDistance = vecX * vecX + vecY * vecY;

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                            }
                        }
                    }
                    break;
                case CellularDistanceFunction.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Vector2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            float vecX = xi - x + vec.X * CellularJitter;
                            float vecY = yi - y + vec.Y * CellularJitter;

                            float newDistance = (Math.Abs(vecX) + Math.Abs(vecY));

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                            }
                        }
                    }
                    break;
                case CellularDistanceFunction.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Vector2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            float vecX = xi - x + vec.X * CellularJitter;
                            float vecY = yi - y + vec.Y * CellularJitter;

                            float newDistance = (Math.Abs(vecX) + Math.Abs(vecY)) + (vecX * vecX + vecY * vecY);

                            if (newDistance < distance)
                            {
                                distance = newDistance;
                                xc = xi;
                                yc = yi;
                            }
                        }
                    }
                    break;
            }

            switch (CellularReturnType)
            {
                case CellularReturnType.CellValue:
                    return ValCoord2D(Seed, xc, yc);

                case CellularReturnType.NoiseLookup:
                    Vector2 vec = Cell2D[Hash2D(Seed, xc, yc) & 255];
                    return CellularNoiseLookup.GetNoise(xc + vec.X * CellularJitter, yc + vec.Y * CellularJitter);

                case CellularReturnType.Distance:
                    return distance;
                default:
                    return 0;
            }
        }

        private float SingleCellular2Edge(float x, float y)
        {
            int xr = FastRound(x);
            int yr = FastRound(y);

            float[] distance = { 999999, 999999, 999999, 999999 };

            switch (CellularDistanceFunction)
            {
                default:
                case CellularDistanceFunction.Euclidean:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Vector2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            float vecX = xi - x + vec.X * CellularJitter;
                            float vecY = yi - y + vec.Y * CellularJitter;

                            float newDistance = vecX * vecX + vecY * vecY;

                            for (int i = _cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }
                    break;
                case CellularDistanceFunction.Manhattan:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Vector2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            float vecX = xi - x + vec.X * CellularJitter;
                            float vecY = yi - y + vec.Y * CellularJitter;

                            float newDistance = Math.Abs(vecX) + Math.Abs(vecY);

                            for (int i = _cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }
                    break;
                case CellularDistanceFunction.Natural:
                    for (int xi = xr - 1; xi <= xr + 1; xi++)
                    {
                        for (int yi = yr - 1; yi <= yr + 1; yi++)
                        {
                            Vector2 vec = Cell2D[Hash2D(Seed, xi, yi) & 255];

                            float vecX = xi - x + vec.X * CellularJitter;
                            float vecY = yi - y + vec.Y * CellularJitter;

                            float newDistance = (Math.Abs(vecX) + Math.Abs(vecY)) + (vecX * vecX + vecY * vecY);

                            for (int i = _cellularDistanceIndex1; i > 0; i--)
                                distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
                            distance[0] = Math.Min(distance[0], newDistance);
                        }
                    }
                    break;
            }

            return CellularReturnType switch
            {
                CellularReturnType.Distance2 => distance[_cellularDistanceIndex1],
                CellularReturnType.Distance2Add => distance[_cellularDistanceIndex1] + distance[_cellularDistanceIndex0],
                CellularReturnType.Distance2Sub => distance[_cellularDistanceIndex1] - distance[_cellularDistanceIndex0],
                CellularReturnType.Distance2Mul => distance[_cellularDistanceIndex1] * distance[_cellularDistanceIndex0],
                CellularReturnType.Distance2Div => distance[_cellularDistanceIndex0] / distance[_cellularDistanceIndex1],
                _ => 0,
            };
        }

        public void GradientPerturb(ref float x, ref float y, ref float z)
        {
            SingleGradientPerturb(Seed, GradientPerturbAmp, Frequency, ref x, ref y, ref z);
        }

        public void GradientPerturbFractal(ref float x, ref float y, ref float z)
        {
            int seed = Seed;
            float amp = GradientPerturbAmp * _fractalBounding;
            float freq = Frequency;

            SingleGradientPerturb(seed, amp, Frequency, ref x, ref y, ref z);

            for (int i = 1; i < _octaves; i++)
            {
                freq *= FractalLacunarity;
                amp *= _gain;
                SingleGradientPerturb(++seed, amp, freq, ref x, ref y, ref z);
            }
        }

        private void SingleGradientPerturb(int seed, float perturbAmp, float frequency, ref float x, ref float y, ref float z)
        {
            float xf = x * frequency;
            float yf = y * frequency;
            float zf = z * frequency;

            int x0 = FastFloor(xf);
            int y0 = FastFloor(yf);
            int z0 = FastFloor(zf);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            int z1 = z0 + 1;

            float xs, ys, zs;
            switch (Interp)
            {
                default:
                case Interp.Linear:
                    xs = xf - x0;
                    ys = yf - y0;
                    zs = zf - z0;
                    break;
                case Interp.Hermite:
                    xs = InterpHermiteFunc(xf - x0);
                    ys = InterpHermiteFunc(yf - y0);
                    zs = InterpHermiteFunc(zf - z0);
                    break;
                case Interp.Quintic:
                    xs = InterpQuinticFunc(xf - x0);
                    ys = InterpQuinticFunc(yf - y0);
                    zs = InterpQuinticFunc(zf - z0);
                    break;
            }

            Vector3 vec0 = Cell3D[Hash3D(seed, x0, y0, z0) & 255];
            Vector3 vec1 = Cell3D[Hash3D(seed, x1, y0, z0) & 255];

            float lx0x = Lerp(vec0.X, vec1.X, xs);
            float ly0x = Lerp(vec0.Y, vec1.Y, xs);
            float lz0x = Lerp(vec0.Z, vec1.Z, xs);

            vec0 = Cell3D[Hash3D(seed, x0, y1, z0) & 255];
            vec1 = Cell3D[Hash3D(seed, x1, y1, z0) & 255];

            float lx1x = Lerp(vec0.X, vec1.X, xs);
            float ly1x = Lerp(vec0.Y, vec1.Y, xs);
            float lz1x = Lerp(vec0.Z, vec1.Z, xs);

            float lx0y = Lerp(lx0x, lx1x, ys);
            float ly0y = Lerp(ly0x, ly1x, ys);
            float lz0y = Lerp(lz0x, lz1x, ys);

            vec0 = Cell3D[Hash3D(seed, x0, y0, z1) & 255];
            vec1 = Cell3D[Hash3D(seed, x1, y0, z1) & 255];

            lx0x = Lerp(vec0.X, vec1.X, xs);
            ly0x = Lerp(vec0.Y, vec1.Y, xs);
            lz0x = Lerp(vec0.Z, vec1.Z, xs);

            vec0 = Cell3D[Hash3D(seed, x0, y1, z1) & 255];
            vec1 = Cell3D[Hash3D(seed, x1, y1, z1) & 255];

            lx1x = Lerp(vec0.X, vec1.X, xs);
            ly1x = Lerp(vec0.Y, vec1.Y, xs);
            lz1x = Lerp(vec0.Z, vec1.Z, xs);

            x += Lerp(lx0y, Lerp(lx0x, lx1x, ys), zs) * perturbAmp;
            y += Lerp(ly0y, Lerp(ly0x, ly1x, ys), zs) * perturbAmp;
            z += Lerp(lz0y, Lerp(lz0x, lz1x, ys), zs) * perturbAmp;
        }

        public void GradientPerturb(ref float x, ref float y)
        {
            SingleGradientPerturb(Seed, GradientPerturbAmp, Frequency, ref x, ref y);
        }

        public void GradientPerturbFractal(ref float x, ref float y)
        {
            int seed = Seed;
            float amp = GradientPerturbAmp * _fractalBounding;
            float freq = Frequency;

            SingleGradientPerturb(seed, amp, Frequency, ref x, ref y);

            for (int i = 1; i < _octaves; i++)
            {
                freq *= FractalLacunarity;
                amp *= _gain;
                SingleGradientPerturb(++seed, amp, freq, ref x, ref y);
            }
        }

        private void SingleGradientPerturb(int seed, float perturbAmp, float frequency, ref float x, ref float y)
        {
            float xf = x * frequency;
            float yf = y * frequency;

            int x0 = FastFloor(xf);
            int y0 = FastFloor(yf);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float xs, ys;
            switch (Interp)
            {
                default:
                case Interp.Linear:
                    xs = xf - x0;
                    ys = yf - y0;
                    break;
                case Interp.Hermite:
                    xs = InterpHermiteFunc(xf - x0);
                    ys = InterpHermiteFunc(yf - y0);
                    break;
                case Interp.Quintic:
                    xs = InterpQuinticFunc(xf - x0);
                    ys = InterpQuinticFunc(yf - y0);
                    break;
            }

            Vector2 vec0 = Cell2D[Hash2D(seed, x0, y0) & 255];
            Vector2 vec1 = Cell2D[Hash2D(seed, x1, y0) & 255];

            float lx0x = Lerp(vec0.X, vec1.X, xs);
            float ly0x = Lerp(vec0.Y, vec1.Y, xs);

            vec0 = Cell2D[Hash2D(seed, x0, y1) & 255];
            vec1 = Cell2D[Hash2D(seed, x1, y1) & 255];

            float lx1x = Lerp(vec0.X, vec1.X, xs);
            float ly1x = Lerp(vec0.Y, vec1.Y, xs);

            x += Lerp(lx0x, lx1x, ys) * perturbAmp;
            y += Lerp(ly0x, ly1x, ys) * perturbAmp;
        }

    }
}
