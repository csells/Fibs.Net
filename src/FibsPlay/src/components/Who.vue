<template>
  <div>
    <template v-if="client.who.length">
      <input placeholder="Filter by name" v-model="nameFilter"></input>
      <input type="checkbox" v-model="playingFilter">playing</input>
      <input type="checkbox" v-model="availableFilter">available</input>
      <input type="checkbox" v-model="humanFilter">human</input>
      <input type="checkbox" v-model="robotFilter">robot</input>
    </template>
    <table>
      <tr v-if="client.who.length">
        <th>name</th>
        <th>opponent</th>
        <!--<th>watching</th>-->
        <th>ready</th>
        <!--<th>away</th>-->
        <th>rating</th>
        <th>experience</th>
        <!--<th>idle</th>
        <th>login</th>
        <th>hostName</th>
        <th>client</th>
        <th>email</th>-->
        <th>action</th>
      </tr>
      <tr v-for="person in people" :key="person.name">
        <td>{{person.name}}</td>
        <td>{{person.opponent}}</td>
        <!--<td>{{person.watching}}</td>-->
        <td>{{person.ready}}</td>
        <!--<td>{{person.away}}</td>-->
        <td>{{person.rating}}</td>
        <td>{{person.experience}}</td>
        <!--<td>{{person.idle}}</td>
        <td>{{person.login}}</td>
        <td>{{person.hostName}}</td>
        <td>{{person.client}}</td>
        <td>{{person.email}}</td>-->
        <td v-if="canPlay(person)"><button>Play</button></td>
        <td v-else-if="canWatch(person)"><button>Watch</button></td>
      </tr>
    </table>
  </div>
</template>

<script>
export default {
  data() {
    return {
      nameFilter: "",
      playingFilter: false,
      availableFilter: false,
      humanFilter: true,
      robotFilter: true,
      client: this.$root.$data.client
    };
  },

  computed: {
    people: function() {
      return [...this.client.who] // copy who array
        .sort((a, b) => a.name.localeCompare(b.name, "en", { sensitivity: "base" })) // case insensitive sort
        .filter(this.filterFn) // sort by user options
      ;
    }
  },

  methods: {
    canPlay: function(person) {
      return person.ready && !person.opponent;
    },

    canWatch: function(person) {
      return !!person.opponent;
    },

    filterFn: function (person) {
      // don't show the currently logged in user
      if (person.name === this.client.session.user) { return false; }
      // if the name doesn't match the filter, we're done
      if (!person.name.toLowerCase().includes(this.nameFilter.toLowerCase())) { return false; }

      // if they're required to be available and they're not, we're done
      let personIsAvailable = person.ready && !person.away && person.opponent === null;
      if (this.availableFilter && !personIsAvailable) { return false; }

      // if they're required to be playing and they're not, we're done
      if (this.playingFilter && person.opponent == null) { return false; }

      // check if the person is a robot (TODO: something better here)
      let personIsRobot = person.name.toLowerCase().includes("bot");

      // if we're not including humans and they're a human, we're done
      if (!this.humanFilter && !personIsRobot) { return false; }

      // if we're not including robots and they're a robot, we're done
      if (!this.robotFilter && personIsRobot) { return false; }

      return true;
    }
  }
};
</script>
