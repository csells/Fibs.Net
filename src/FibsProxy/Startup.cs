using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FibsProxy {
  public class FibsProxyOptions {
    public string DefaultServer { get; set; } = "fibs.com";
    public int DefaultPort { get; set; } = 4321;
  }

  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
      services.AddOptions();
      services.Configure<FibsProxyOptions>(Configuration.GetSection("FibsProxy"));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
      if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }
      app.Map("/fibs", SocketHandler.Map);
    }
  }
}
