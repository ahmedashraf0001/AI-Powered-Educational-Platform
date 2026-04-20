const fs = require('fs');
const content = fs.readFileSync('src/pages/public/InstructorProfilePage.tsx', 'utf8');

const regex = /<div className="grid grid-cols-1 md:grid-cols-3 gap-8 text-sm">([\s\S]*?)<\/CardContent>/;

const replacement = `{hasAboutOrContact ? (
              <div className="grid grid-cols-1 md:grid-cols-3 gap-8 text-sm pt-6 mt-6 border-t border-border/50">     
                <div className="md:col-span-2 space-y-4">
                  {profile.bio && (
                    <div>
                      <h3 className="font-semibold text-base mb-2">About Me</h3>  
                      <p className="text-muted-foreground whitespace-pre-wrap leading-relaxed">
                        {profile.bio}
                      </p>
                    </div>
                  )}
                </div>
                {hasContactInfo && (
                  <div className="space-y-4">
                    <h3 className="font-semibold text-base mb-2">Contact & Info</h3>
                    <ul className="space-y-3 text-muted-foreground">
                      {profile.location && (
                        <li className="flex items-center gap-3">
                          <MapPin className="h-4 w-4 text-primary/70" /> <span>{profile.location}</span>
                        </li>
                      )}
                      {profile.website && (
                        <li className="flex items-center gap-3 flex-wrap">
                          <Globe className="h-4 w-4 text-primary/70 shrink-0" />
                          <a href={profile.website} target="_blank" rel="noopener noreferrer" className="hover:text-primary transition-colors underline-offset-4 hover:underline break-all">
                            {profile.website.replace(/^https?:\\/\\//, '')}
                          </a>
                        </li>
                      )}
                      {profile.linkedInUrl && (
                        <li className="flex items-center gap-3 flex-wrap">
                          <Linkedin className="h-4 w-4 text-primary/70 shrink-0" />
                          <a href={profile.linkedInUrl} target="_blank" rel="noopener noreferrer" className="hover:text-primary transition-colors underline-offset-4 hover:underline break-all">
                            LinkedIn Profile
                          </a>
                        </li>
                      )}
                    </ul>
                  </div>
                )}
              </div>
            ) : (
              <div className="text-center py-10 mt-6 border-t border-border/50">
                <p className="text-muted-foreground">This user hasn't added any details to their profile yet.</p>
              </div>
            )}
          </CardContent>`;

if(content.match(regex)) {
   fs.writeFileSync('src/pages/public/InstructorProfilePage.tsx', content.replace(regex, replacement));
   console.log('Replaced successfully.');
} else {
   console.log('Regex did not match.');
}
