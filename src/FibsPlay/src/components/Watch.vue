<template>
  <div>
    <h1>watch {{$route.params.name}}</h1>
    <svg version="1.1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 574 420" height="400">
      <!-- frame -->
      <rect x="0" y="0" height="420" width="574" fill="white" stroke="black" stroke-width="5" />
      
      <!-- outer board -->
      <rect x="20" y="20" height="380" width="216" fill="darkgreen" stroke="black" />

       <!-- inner board -->
      <rect x="284" y="20" height="380" width="216" fill="darkgreen" stroke="black" />

      <!-- pips: top-left -->
      <polygon points="20,20 38,170 54,20" fill="gainsboro" stroke="black" />
      <polygon points="56,20 74,170 90,20" fill="grey" stroke="black" />
      <polygon points="92,20 110,170 126,20" fill="gainsboro" stroke="black" />
      <polygon points="128,20 146,170 162,20" fill="grey" stroke="black" />
      <polygon points="164,20 182,170 200,20" fill="gainsboro" stroke="black" />
      <polygon points="202,20 220,170 236,20" fill="grey" stroke="black" />

      <!-- pips: top-right -->
      <polygon points="284,20 302,170 318,20" fill="gainsboro" stroke="black" />
      <polygon points="320,20 338,170 354,20" fill="grey" stroke="black" />
      <polygon points="356,20 374,170 390,20" fill="gainsboro" stroke="black" />
      <polygon points="392,20 410,170 426,20" fill="grey" stroke="black" />
      <polygon points="428,20 446,170 462,20" fill="gainsboro" stroke="black" />
      <polygon points="464,20 482,170 498,20" fill="grey" stroke="black" />

      <!-- pips: bottom-left -->
      <polygon points="20,400 38,250 54,400" fill="grey" stroke="black" />
      <polygon points="56,400 74,250 90,400" fill="gainsboro" stroke="black" />
      <polygon points="92,400 110,250 126,400" fill="grey" stroke="black" />
      <polygon points="128,400 146,250 162,400" fill="gainsboro" stroke="black" />
      <polygon points="164,400 182,250 200,400" fill="grey" stroke="black" />
      <polygon points="202,400 220,250 236,400" fill="gainsboro" stroke="black" />

      <!-- pips: bottom-right -->
      <polygon points="284,400 302,250 318,400" fill="grey" stroke="black" />
      <polygon points="320,400 338,250 354,400" fill="gainsboro" stroke="black" />
      <polygon points="356,400 374,250 390,400" fill="grey" stroke="black" />
      <polygon points="392,400 410,250 426,400" fill="gainsboro" stroke="black" />
      <polygon points="428,400 446,250 462,400" fill="grey" stroke="black" />
      <polygon points="464,400 482,250 498,400" fill="gainsboro" stroke="black" />

      <!-- text: top-left -->
      <text x="38" y="17" font-family="arial" text-anchor="middle">13</text>
      <text x="74" y="17" font-family="arial" text-anchor="middle">14</text>
      <text x="110" y="17" font-family="arial" text-anchor="middle">15</text>
      <text x="146" y="17" font-family="arial" text-anchor="middle">16</text>
      <text x="182" y="17" font-family="arial" text-anchor="middle">17</text>
      <text x="218" y="17" font-family="arial" text-anchor="middle">18</text>

      <!-- text: top-right -->
      <text x="302" y="17" font-family="arial" text-anchor="middle">19</text>
      <text x="338" y="17" font-family="arial" text-anchor="middle">20</text>
      <text x="374" y="17" font-family="arial" text-anchor="middle">21</text>
      <text x="410" y="17" font-family="arial" text-anchor="middle">22</text>
      <text x="446" y="17" font-family="arial" text-anchor="middle">23</text>
      <text x="482" y="17" font-family="arial" text-anchor="middle">24</text>

      <!-- text: bottom-left -->
      <text x="38" y="414" font-family="arial" text-anchor="middle">12</text>
      <text x="74" y="414" font-family="arial" text-anchor="middle">11</text>
      <text x="110" y="414" font-family="arial" text-anchor="middle">10</text>
      <text x="146" y="414" font-family="arial" text-anchor="middle">9</text>
      <text x="182" y="414" font-family="arial" text-anchor="middle">8</text>
      <text x="218" y="414" font-family="arial" text-anchor="middle">7</text>

      <!-- text: bottom-right -->
      <text x="302" y="414" font-family="arial" text-anchor="middle">6</text>
      <text x="338" y="414" font-family="arial" text-anchor="middle">5</text>
      <text x="374" y="414" font-family="arial" text-anchor="middle">4</text>
      <text x="410" y="414" font-family="arial" text-anchor="middle">3</text>
      <text x="446" y="414" font-family="arial" text-anchor="middle">2</text>
      <text x="482" y="414" font-family="arial" text-anchor="middle">1</text>

      <!-- player1 home -->
      <rect x="520" y="220" height="180" width="32" fill="darkgreen" stroke="black" stroke-width="2" />

      <!-- player2 home -->
      <rect x="520" y="20" height="180" width="32" fill="darkgreen" stroke="black" stroke-width="2" />

      <!-- pieces -->
      <circle v-for="p in pieces" :cx="p.pos.cx" :cy="p.pos.cy" r="14" :fill="p.color" stroke="black" stroke-width="2px" />

    </svg>
  </div>
</template>

<script>
let cysTop = [35, 64, 93, 122, 151];
let cysBottom = [385, 356, 327, 298, 269];

let poses = [
  { cx: 260, cys: [164, 132, 100] }, // player 1 bar

  // bottom
  { cx: 481, cys: cysBottom }, // pip 1
  { cx: 445, cys: cysBottom }, // pip 2
  { cx: 409, cys: cysBottom }, // pip 3
  { cx: 373, cys: cysBottom }, // pip 4
  { cx: 337, cys: cysBottom }, // pip 5
  { cx: 301, cys: cysBottom }, // pip 6
  { cx: 219, cys: cysBottom }, // pip 7
  { cx: 182, cys: cysBottom }, // pip 8
  { cx: 145, cys: cysBottom }, // pip 9
  { cx: 109, cys: cysBottom }, // pip 10
  { cx: 73, cys: cysBottom }, // pip 11
  { cx: 37, cys: cysBottom }, // pip 12

  // top
  { cx: 37, cys: cysTop }, // pip 13
  { cx: 73, cys: cysTop }, // pip 14
  { cx: 109, cys: cysTop }, // pip 15
  { cx: 145, cys: cysTop }, // pip 16
  { cx: 182, cys: cysTop }, // pip 17
  { cx: 219, cys: cysTop }, // pip 18
  { cx: 301, cys: cysTop }, // pip 19
  { cx: 337, cys: cysTop }, // pip 20
  { cx: 373, cys: cysTop }, // pip 21
  { cx: 409, cys: cysTop }, // pip 22
  { cx: 445, cys: cysTop }, // pip 23
  { cx: 481, cys: cysTop }, // pip 24

  { cx: 260, cys: [256, 288, 320] }, // player 2 bar
];

export default {
  data() {
    return {
      client: this.$root.$data.client,
    };
  },

  computed: {
    // 26 numbers giving the board. Positions 0 and 25 are documented to represent the bars
    // for the players, but they don't seem to match the other spot designated as the
    // place with the bars (player1Bar, player2Bar), so should be ignored.
    // Positive numbers represent 0's pieces and negative numbers represent X's pieces.
    // e.g. 0:-2:0:0:0:0:5:0:3:0:0:0:-5:5:0:0:0:-3:0:-5:0:0:0:0:2:0
    // from http://www.fibs.com/fibs_interface.html#board_state
    pieces: function() {
      if (!this.client.watching.board) { return; }

      let pieces = [];
      this.client.watching.board.split(":").forEach((ch, pip) => {
        let count = parseInt(ch); // piece count for the current pip
        let maxCount = poses[pip].cys.length;
        for (let i = 0; i !== Math.min(Math.abs(count), maxCount); ++i) {
          let pos = { cx: poses[pip].cx, cy: poses[pip].cys[i] };
          pieces.push({ pos: pos, color: count < 0 ? "black" : "white" });
        }
      });

      console.log(`pieces: ${pieces.length}`);
      return pieces;
    }
  },

  // this seems to happen every time the route shows this component
  mounted: function(e) {
    console.log(`mounted: ${this.$route.params.name}`);
    this.client.look(this.$route.params.name); // get initial board
    this.client.watch(this.$route.params.name); // get notifications on board changes
  },

  // doesn't seem to do anything...
  // watch: {
  //   '$route': function(newRoute) {
  //     console.log(`$route changed: ${this.$route.params.name}`);
  //   }
  // }
};
</script>
