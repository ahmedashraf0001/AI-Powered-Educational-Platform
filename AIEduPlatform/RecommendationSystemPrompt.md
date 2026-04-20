prequisets before implementing the recommendation system:

we need tag pool that will be automatically generated for each added courses based on titles and context (tags creation should be a part of the indexing).

tag pool can then be used as skill tags that the user interested in learning.

prompt:
A centralized set of normalized tags generated from course content 
(title, description, metadata) using NLP/keyword extraction.

Each course → mapped to multiple tags  
Each user → mapped to interest tags (derived from behavior)

tags assignation for users should be manually and automated based on enrolled courses(if the user enrolled to a course all its tags will be assigned to him)

recommendation system:

will be sectioned into 4 sections -> 


Top Picks For You (most important):

we first choose candidate courses:
courses that are similar to courses you watched,
popular courses,
new courses

note courses that already enrolled in or completed are exception from the candidate

note limit candidates to 100 courses.

score calculations:

Similarity Score: for each course we compare user interest with the course using similarity in tags, user bio and other features and create a similarity score (embedding for similarity scoring here).
Similarity = cosine(user_embedding, course_embedding)

example: 
User likes: C#, SQL
Course: ASP.NET (C#)

→ high similarity = 0.9

Course Quality score: using the avg rating for the course and the completion rate 

ex: 
QualityScore = (Rating * 0.7 + CompletionRate * 0.3)


Popularity Score: by the number of enrollments, views.

Recency score: new courses gets boost 
RecentScore = e^(-days_since_release)


note all scores should be normalized for consistency (All scores MUST be between 0 and 1)
Similarity → already 0–1 
Rating → divide by 5
Popularity → log normalize
Recency → already normalized

then the final score will be the summation of all that 
Score =
  (0.4 * Similarity)
+ (0.2 * Quality)
+ (0.15 * Popularity)
+ (0.15 * Recency)


sort the courses in descending and return top 10.


in the recommendation system there should be randomness that means the response of the system should be 
80% → best score
20% → random from candidates

----------------------------------------------------------------------------------------------------------------------------------------

Continue Learning -> courses you didn't finish.

Because You Learned X -> You already implicitly support it via similarity, but this makes it explicit and explainable (find similar courses).

Top courses -> popularity-based (views, enrollments)

Trending courses → time-based popularity and high engagement growth



