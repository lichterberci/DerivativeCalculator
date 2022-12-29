import React, { Component } from 'react';

export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor(props) {
    super(props);
      this.state = {
          data: {}
      };
  }

  componentDidMount() {
    this.populateWeatherData();
  }

   render() {
       return (<>
           {
               JSON.stringify(this.state.data, null, 4)
           }
       </>)
  }

  async populateWeatherData() {
    const response = await fetch('weatherforecast');
      const data = await response.json();
      this.setState({ data: data });
  }
}
